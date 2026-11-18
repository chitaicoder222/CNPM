// Purpose: Provides protected dashboard, device, POI and topic management endpoints for the VIVOICE admin UI.
// Responsibility: Validates administrative writes, persists them in SQL Server and records safe audit events.
using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vivoice.Web.Data;
using Vivoice.Web.Data.Entities;
using Vivoice.Web.Models;
using Vivoice.Web.Services;

namespace Vivoice.Web.Controllers;

[ApiController]
[AllowAnonymous]
[AutoValidateAntiforgeryToken]
[Route("api/admin")]
public sealed class AdminController(VivoiceDbContext db, AnalyticsService analytics) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> Dashboard([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var end = to?.ToUniversalTime() ?? DateTime.UtcNow;
        var start = from?.ToUniversalTime() ?? end.AddDays(-6);
        if (!DashboardRangePolicy.IsValid(start, end))
        {
            return BadRequest(new { code = "INVALID_DATE_RANGE", message = "Thời điểm bắt đầu phải không muộn hơn thời điểm kết thúc." });
        }

        return Ok(await analytics.GetDashboardAsync(start, end, cancellationToken));
    }

    [HttpGet("devices/active")]
    public async Task<IActionResult> ActiveDevices(CancellationToken cancellationToken)
    {
        return Ok(await analytics.GetActiveDevicesAsync(cancellationToken));
    }

    [HttpGet("pois")]
    public async Task<IActionResult> GetPois(CancellationToken cancellationToken)
    {
        var pois = await db.Pois.AsNoTracking().Include(x => x.Translations).OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken);
        return Ok(pois.Select(poi => ToAdminPoi(poi)));
    }

    [HttpPost("pois")]
    public async Task<IActionResult> CreatePoi(PoiWriteRequest request, CancellationToken cancellationToken)
    {
        var validation = ValidatePoi(request);
        if (validation is not null) return validation;
        if (await db.Pois.AnyAsync(x => x.Slug == request.Slug.Trim().ToLowerInvariant(), cancellationToken)) return Conflict(new { code = "SLUG_CONFLICT", message = "Slug đã tồn tại." });
        var poi = BuildPoi(request);
        db.Pois.Add(poi);
        await db.SaveChangesAsync(cancellationToken);
        await AuditAsync("POI_CREATE", "Poi", poi.Id.ToString(), cancellationToken);
        return Created($"/api/admin/pois/{poi.Id}", ToAdminPoi(poi));
    }

    [HttpPatch("pois/{id:long}")]
    public async Task<IActionResult> UpdatePoi(long id, PoiWriteRequest request, CancellationToken cancellationToken)
    {
        var validation = ValidatePoi(request);
        if (validation is not null) return validation;
        var poi = await db.Pois.Include(x => x.Translations).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (poi is null) return NotFound(new { code = "POI_NOT_FOUND", message = "Không tìm thấy POI." });
        if (await db.Pois.AnyAsync(x => x.Id != id && x.Slug == request.Slug.Trim().ToLowerInvariant(), cancellationToken)) return Conflict(new { code = "SLUG_CONFLICT", message = "Slug đã tồn tại." });
        poi.Slug = request.Slug.Trim().ToLowerInvariant(); poi.Latitude = request.Latitude; poi.Longitude = request.Longitude; poi.TriggerRadiusMeters = request.TriggerRadiusMeters; poi.ImageUrl = request.ImageUrl?.Trim(); poi.DisplayOrder = request.DisplayOrder; poi.IsActive = request.IsActive; poi.UpdatedAtUtc = DateTime.UtcNow;
        var vi = poi.Translations.FirstOrDefault(x => x.LanguageCode == "vi");
        if (vi is null) poi.Translations.Add(new PoiTranslation { LanguageCode = "vi", Name = request.NameVi.Trim(), Summary = request.SummaryVi.Trim() });
        else { vi.Name = request.NameVi.Trim(); vi.Summary = request.SummaryVi.Trim(); }
        await db.SaveChangesAsync(cancellationToken);
        await AuditAsync("POI_UPDATE", "Poi", poi.Id.ToString(), cancellationToken);
        return Ok(ToAdminPoi(poi));
    }

    [HttpGet("pois/{poiId:long}/topics")]
    public async Task<IActionResult> GetTopics(long poiId, CancellationToken cancellationToken)
    {
        var topics = await db.PoiTopics.AsNoTracking().Where(x => x.PoiId == poiId).Include(x => x.Translations).OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken);
        return Ok(topics.Select(ToAdminTopic));
    }

    [HttpPost("pois/{poiId:long}/topics")]
    public async Task<IActionResult> CreateTopic(long poiId, TopicWriteRequest request, CancellationToken cancellationToken)
    {
        if (!IsValidTopic(request)) return BadRequest(new { code = "VALIDATION_ERROR", message = "Category, title và nội dung tiếng Việt là bắt buộc." });
        if (!await db.Pois.AnyAsync(x => x.Id == poiId, cancellationToken)) return NotFound(new { code = "POI_NOT_FOUND", message = "Không tìm thấy POI." });
        var category = TopicCategoryPolicy.Normalize(request.Category)!;
        if (await db.PoiTopics.AnyAsync(x => x.PoiId == poiId && x.Category == category, cancellationToken)) return Conflict(new { code = "TOPIC_CATEGORY_CONFLICT", message = "POI đã có chủ đề thuộc nhóm này." });
        var topic = new PoiTopic { PoiId = poiId, Category = category, DisplayOrder = request.DisplayOrder, IsActive = request.IsActive, Translations = [new TopicTranslation { LanguageCode = "vi", Title = request.TitleVi.Trim(), Content = request.ContentVi.Trim() }] };
        db.PoiTopics.Add(topic); await db.SaveChangesAsync(cancellationToken); await AuditAsync("TOPIC_CREATE", "PoiTopic", topic.Id.ToString(), cancellationToken);
        return Created($"/api/admin/pois/{poiId}/topics/{topic.Id}", ToAdminTopic(topic));
    }

    [HttpPatch("topics/{topicId:long}")]
    public async Task<IActionResult> UpdateTopic(long topicId, TopicWriteRequest request, CancellationToken cancellationToken)
    {
        if (!IsValidTopic(request)) return BadRequest(new { code = "VALIDATION_ERROR", message = "Category, title và nội dung tiếng Việt là bắt buộc." });
        var topic = await db.PoiTopics.Include(x => x.Translations).FirstOrDefaultAsync(x => x.Id == topicId, cancellationToken);
        if (topic is null) return NotFound(new { code = "TOPIC_NOT_FOUND", message = "Không tìm thấy chủ đề." });
        var category = TopicCategoryPolicy.Normalize(request.Category)!;
        if (await db.PoiTopics.AnyAsync(x => x.Id != topicId && x.PoiId == topic.PoiId && x.Category == category, cancellationToken)) return Conflict(new { code = "TOPIC_CATEGORY_CONFLICT", message = "POI đã có chủ đề thuộc nhóm này." });
        topic.Category = category; topic.DisplayOrder = request.DisplayOrder; topic.IsActive = request.IsActive;
        var vi = topic.Translations.FirstOrDefault(x => x.LanguageCode == "vi");
        if (vi is null) topic.Translations.Add(new TopicTranslation { LanguageCode = "vi", Title = request.TitleVi.Trim(), Content = request.ContentVi.Trim() });
        else { vi.Title = request.TitleVi.Trim(); vi.Content = request.ContentVi.Trim(); }
        await db.SaveChangesAsync(cancellationToken); await AuditAsync("TOPIC_UPDATE", "PoiTopic", topic.Id.ToString(), cancellationToken);
        return Ok(ToAdminTopic(topic));
    }

    [HttpGet("pois/{poiId:long}/translations")]
    public async Task<IActionResult> GetPoiTranslations(long poiId, CancellationToken cancellationToken)
    {
        var translations = await db.PoiTranslations.AsNoTracking().Where(x => x.PoiId == poiId).OrderBy(x => x.LanguageCode)
            .Select(x => new { x.LanguageCode, x.Name, x.Summary }).ToListAsync(cancellationToken);
        return Ok(translations);
    }

    [HttpPut("pois/{poiId:long}/translations/{languageCode}")]
    public async Task<IActionResult> PutPoiTranslation(long poiId, string languageCode, PoiTranslationWriteRequest request, CancellationToken cancellationToken)
    {
        var language = NormalizeLanguage(languageCode);
        if (language is null || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Summary)) return BadRequest(new { code = "VALIDATION_ERROR", message = "Ngôn ngữ, tên và mô tả là bắt buộc." });
        if (!await db.Pois.AnyAsync(x => x.Id == poiId, cancellationToken)) return NotFound(new { code = "POI_NOT_FOUND", message = "Không tìm thấy POI." });
        var translation = await db.PoiTranslations.FirstOrDefaultAsync(x => x.PoiId == poiId && x.LanguageCode == language, cancellationToken);
        if (translation is null) db.PoiTranslations.Add(new PoiTranslation { PoiId = poiId, LanguageCode = language, Name = request.Name.Trim(), Summary = request.Summary.Trim() });
        else { translation.Name = request.Name.Trim(); translation.Summary = request.Summary.Trim(); }
        await db.SaveChangesAsync(cancellationToken); await AuditAsync("POI_TRANSLATION_UPSERT", "Poi", poiId.ToString(), cancellationToken);
        return NoContent();
    }

    [HttpGet("topics/{topicId:long}/translations")]
    public async Task<IActionResult> GetTopicTranslations(long topicId, CancellationToken cancellationToken)
    {
        var translations = await db.TopicTranslations.AsNoTracking().Where(x => x.PoiTopicId == topicId).OrderBy(x => x.LanguageCode)
            .Select(x => new { x.LanguageCode, x.Title, x.Content, x.IsPublished }).ToListAsync(cancellationToken);
        return Ok(translations);
    }

    [HttpPut("topics/{topicId:long}/translations/{languageCode}")]
    public async Task<IActionResult> PutTopicTranslation(long topicId, string languageCode, TopicTranslationWriteRequest request, CancellationToken cancellationToken)
    {
        var language = NormalizeLanguage(languageCode);
        if (language is null || string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content)) return BadRequest(new { code = "VALIDATION_ERROR", message = "Ngôn ngữ, tiêu đề và nội dung là bắt buộc." });
        if (!await db.PoiTopics.AnyAsync(x => x.Id == topicId, cancellationToken)) return NotFound(new { code = "TOPIC_NOT_FOUND", message = "Không tìm thấy chủ đề." });
        var translation = await db.TopicTranslations.FirstOrDefaultAsync(x => x.PoiTopicId == topicId && x.LanguageCode == language, cancellationToken);
        if (translation is null) db.TopicTranslations.Add(new TopicTranslation { PoiTopicId = topicId, LanguageCode = language, Title = request.Title.Trim(), Content = request.Content.Trim(), IsPublished = request.IsPublished });
        else { translation.Title = request.Title.Trim(); translation.Content = request.Content.Trim(); translation.IsPublished = request.IsPublished; }
        await db.SaveChangesAsync(cancellationToken); await AuditAsync("TOPIC_TRANSLATION_UPSERT", "PoiTopic", topicId.ToString(), cancellationToken);
        return NoContent();
    }

    [HttpGet("advertisements")]
    public async Task<IActionResult> GetAdvertisements(CancellationToken cancellationToken)
    {
        var advertisements = await db.Advertisements.AsNoTracking().Include(x => x.Sponsor).OrderBy(x => x.Placement).ThenBy(x => x.DisplayOrder)
            .Select(x => new { x.Id, x.SponsorId, SponsorName = x.Sponsor == null ? null : x.Sponsor.Name, x.Name, x.ImageUrl, x.TargetUrl, x.Placement, x.StartsAtUtc, x.EndsAtUtc, x.DisplayOrder, x.IsActive })
            .ToListAsync(cancellationToken);
        return Ok(advertisements);
    }

    [HttpPost("advertisements")]
    public async Task<IActionResult> CreateAdvertisement(AdvertisementWriteRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateAdvertisementAsync(request, cancellationToken); if (validation is not null) return validation;
        var advertisement = BuildAdvertisement(request); db.Advertisements.Add(advertisement);
        await db.SaveChangesAsync(cancellationToken); await AuditAsync("ADVERTISEMENT_CREATE", "Advertisement", advertisement.Id.ToString(), cancellationToken);
        return Created($"/api/admin/advertisements/{advertisement.Id}", new { advertisement.Id });
    }

    [HttpPatch("advertisements/{advertisementId:long}")]
    public async Task<IActionResult> UpdateAdvertisement(long advertisementId, AdvertisementWriteRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateAdvertisementAsync(request, cancellationToken); if (validation is not null) return validation;
        var advertisement = await db.Advertisements.FirstOrDefaultAsync(x => x.Id == advertisementId, cancellationToken);
        if (advertisement is null) return NotFound(new { code = "ADVERTISEMENT_NOT_FOUND", message = "Không tìm thấy quảng cáo." });
        ApplyAdvertisement(advertisement, request); await db.SaveChangesAsync(cancellationToken); await AuditAsync("ADVERTISEMENT_UPDATE", "Advertisement", advertisement.Id.ToString(), cancellationToken);
        return Ok(new { advertisement.Id });
    }

    [HttpGet("sponsors")]
    public async Task<IActionResult> GetSponsors(CancellationToken cancellationToken)
    {
        var sponsors = await db.Sponsors.AsNoTracking().OrderBy(x => x.Name).Select(x => new
        {
            x.Id, x.Name, x.ContactNote, x.IsActive,
            AgreementCount = x.Agreements.Count,
            ContractedAmount = x.Agreements.Where(a => a.Status != "cancelled" && a.BillingMode == SponsorshipRevenuePolicy.FixedBillingMode).Sum(a => (decimal?)a.CommittedAmount) ?? 0m,
            PerVisitCampaignCount = x.Agreements.Count(a => a.BillingMode == SponsorshipRevenuePolicy.PerVisitBillingMode)
        }).ToListAsync(cancellationToken);
        return Ok(sponsors);
    }

    [HttpPost("sponsors")]
    public async Task<IActionResult> CreateSponsor(SponsorWriteRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { code = "VALIDATION_ERROR", message = "Tên đối tác là bắt buộc." });
        var sponsor = new Sponsor { Name = request.Name.Trim(), ContactNote = request.ContactNote?.Trim(), IsActive = request.IsActive };
        db.Sponsors.Add(sponsor); await db.SaveChangesAsync(cancellationToken); await AuditAsync("SPONSOR_CREATE", "Sponsor", sponsor.Id.ToString(), cancellationToken);
        return Created($"/api/admin/sponsors/{sponsor.Id}", new { sponsor.Id });
    }

    [HttpPost("sponsor-campaigns")]
    public async Task<IActionResult> CreateSponsorCampaign(SponsorCampaignWriteRequest request, CancellationToken cancellationToken)
    {
        var validation = ValidateSponsorCampaign(request);
        if (validation is not null) return validation;

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var sponsor = new Sponsor
        {
            Name = request.SponsorName.Trim(),
            ContactNote = request.ContactNote?.Trim(),
            IsActive = request.SponsorIsActive
        };
        var agreement = BuildAgreement(request.CampaignName, request.BillingMode, request.CommittedAmount, request.Currency, request.Status, request.StartsAtUtc, request.EndsAtUtc);
        AddInitialRate(agreement, request.BillingMode, request.InitialRateEnabled, request.InitialRateAmount, request.InitialRateEffectiveFromUtc);
        sponsor.Agreements.Add(agreement);
        db.Sponsors.Add(sponsor);
        await db.SaveChangesAsync(cancellationToken);
        await AuditAsync("SPONSOR_CAMPAIGN_CREATE", "SponsorshipAgreement", agreement.Id.ToString(), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Created($"/api/admin/sponsorship-agreements/{agreement.Id}", new { SponsorId = sponsor.Id, AgreementId = agreement.Id });
    }

    [HttpPatch("sponsors/{sponsorId:long}")]
    public async Task<IActionResult> UpdateSponsor(long sponsorId, SponsorWriteRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { code = "VALIDATION_ERROR", message = "Tên đối tác là bắt buộc." });
        var sponsor = await db.Sponsors.FirstOrDefaultAsync(x => x.Id == sponsorId, cancellationToken);
        if (sponsor is null) return NotFound(new { code = "SPONSOR_NOT_FOUND", message = "Không tìm thấy đối tác." });
        sponsor.Name = request.Name.Trim(); sponsor.ContactNote = request.ContactNote?.Trim(); sponsor.IsActive = request.IsActive;
        await db.SaveChangesAsync(cancellationToken); await AuditAsync("SPONSOR_UPDATE", "Sponsor", sponsor.Id.ToString(), cancellationToken);
        return Ok(new { sponsor.Id });
    }

    [HttpGet("sponsorship-agreements")]
    public async Task<IActionResult> GetAgreements(CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        var agreements = await db.SponsorshipAgreements.AsNoTracking()
            .Include(x => x.Sponsor)
            .Include(x => x.Payments)
            .Include(x => x.VisitRates)
            .OrderByDescending(x => x.StartsAtUtc)
            .ToListAsync(cancellationToken);
        return Ok(agreements.Select(agreement => ToAgreementDto(agreement, nowUtc)));
    }

    [HttpPost("sponsorship-agreements")]
    public async Task<IActionResult> CreateAgreement(SponsorshipAgreementWriteRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateAgreementAsync(request, true, cancellationToken); if (validation is not null) return validation;
        var agreement = BuildAgreement(request.CampaignName, request.BillingMode, request.CommittedAmount, request.Currency, request.Status, request.StartsAtUtc, request.EndsAtUtc);
        agreement.SponsorId = request.SponsorId;
        AddInitialRate(agreement, request.BillingMode, request.InitialRateEnabled, request.InitialRateAmount, request.InitialRateEffectiveFromUtc);
        db.SponsorshipAgreements.Add(agreement); await db.SaveChangesAsync(cancellationToken); await AuditAsync("AGREEMENT_CREATE", "SponsorshipAgreement", agreement.Id.ToString(), cancellationToken);
        return Created($"/api/admin/sponsorship-agreements/{agreement.Id}", new { agreement.Id });
    }

    [HttpPatch("sponsorship-agreements/{agreementId:long}")]
    public async Task<IActionResult> UpdateAgreement(long agreementId, SponsorshipAgreementWriteRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateAgreementAsync(request, false, cancellationToken); if (validation is not null) return validation;
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var agreement = await db.SponsorshipAgreements.Include(x => x.Payments).Include(x => x.VisitRates).FirstOrDefaultAsync(x => x.Id == agreementId, cancellationToken);
        if (agreement is null) return NotFound(new { code = "AGREEMENT_NOT_FOUND", message = "Không tìm thấy hợp đồng." });
        if (agreement.SponsorId != request.SponsorId)
            return Conflict(new { code = "SPONSOR_LOCKED", message = "Không thể chuyển campaign đã tạo sang nhà tài trợ khác vì cần bảo toàn lịch sử." });
        var requestedBillingMode = SponsorshipRevenuePolicy.NormalizeBillingMode(request.BillingMode)!;
        if (agreement.BillingMode != requestedBillingMode && (agreement.VisitRates.Count > 0 || await db.VisitSponsorshipRevenues.AnyAsync(revenue => revenue.SponsorshipAgreementId == agreementId, cancellationToken)))
            return Conflict(new { code = "BILLING_MODE_LOCKED", message = "Không thể đổi kiểu tính tiền sau khi campaign đã có lịch sử giá hoặc doanh thu lượt truy cập." });
        if (requestedBillingMode == SponsorshipRevenuePolicy.FixedBillingMode && SponsorshipPaymentPolicy.IsCommitmentBelowReceived(request.CommittedAmount, agreement.Payments.Sum(payment => payment.ReceivedAmount)))
            return Conflict(new { code = "COMMITMENT_CONFLICT", message = "Giá trị hợp đồng không thể thấp hơn tổng khoản đã nhận." });
        var proposedStartsAtUtc = request.StartsAtUtc.ToUniversalTime();
        var proposedEndsAtUtc = request.EndsAtUtc?.ToUniversalTime();
        if (agreement.VisitRates.Any(rate => !SponsorshipRevenuePolicy.IsRateIntervalWithinCampaign(rate.EffectiveFromUtc, rate.EffectiveToUtc, proposedStartsAtUtc, proposedEndsAtUtc)))
            return Conflict(new { code = "RATE_TIME_RANGE_CONFLICT", message = "Thời hạn campaign mới không được loại bỏ bất kỳ mốc giá lịch sử nào." });
        ApplyAgreement(agreement, request.CampaignName, request.BillingMode, request.CommittedAmount, request.Currency, request.Status, request.StartsAtUtc, request.EndsAtUtc);
        await db.SaveChangesAsync(cancellationToken); await AuditAsync("AGREEMENT_UPDATE", "SponsorshipAgreement", agreement.Id.ToString(), cancellationToken); await transaction.CommitAsync(cancellationToken);
        return Ok(new { agreement.Id });
    }

    [HttpPost("sponsorship-agreements/{agreementId:long}/payments")]
    public async Task<IActionResult> AddPayment(long agreementId, SponsorshipPaymentWriteRequest request, CancellationToken cancellationToken)
    {
        if (request.ReceivedAmount <= 0 || request.ReceivedAtUtc == default) return BadRequest(new { code = "VALIDATION_ERROR", message = "Số tiền nhận và thời điểm nhận phải hợp lệ." });
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var agreement = await db.SponsorshipAgreements.Include(x => x.Payments).FirstOrDefaultAsync(x => x.Id == agreementId, cancellationToken);
        if (agreement is null) return NotFound(new { code = "AGREEMENT_NOT_FOUND", message = "Không tìm thấy hợp đồng." });
        var validation = SponsorshipPaymentPolicy.Validate(agreement.Status, agreement.BillingMode, agreement.CommittedAmount, agreement.Payments.Sum(payment => payment.ReceivedAmount), request.ReceivedAmount);
        if (validation == SponsorshipPaymentValidation.AgreementCancelled)
            return Conflict(new { code = "AGREEMENT_CANCELLED", message = "Không thể ghi nhận thanh toán cho hợp đồng đã hủy." });
        if (validation == SponsorshipPaymentValidation.BillingModeNotEligible)
            return Conflict(new { code = "PAYMENT_ONLY_FOR_FIXED_CAMPAIGN", message = "Khoản tiền chỉ được ghi nhận cho campaign hợp đồng cố định. Doanh thu campaign theo lượt được tính tự động từ lượt truy cập." });
        if (validation == SponsorshipPaymentValidation.CommitmentExceeded)
            return Conflict(new { code = "PAYMENT_EXCEEDS_COMMITMENT", message = "Khoản thanh toán vượt giá trị hợp đồng còn lại." });
        var payment = new SponsorshipPayment { SponsorshipAgreementId = agreementId, ReceivedAmount = request.ReceivedAmount, ReceivedAtUtc = request.ReceivedAtUtc.ToUniversalTime() };
        db.SponsorshipPayments.Add(payment); await db.SaveChangesAsync(cancellationToken); await AuditAsync("PAYMENT_CREATE", "SponsorshipPayment", payment.Id.ToString(), cancellationToken); await transaction.CommitAsync(cancellationToken);
        return Created($"/api/admin/sponsorship-agreements/{agreementId}/payments/{payment.Id}", new { payment.Id });
    }

    [HttpGet("sponsorship-agreements/{agreementId:long}/visit-rate-periods")]
    public async Task<IActionResult> GetAgreementVisitRatePeriods(long agreementId, CancellationToken cancellationToken)
    {
        if (!await db.SponsorshipAgreements.AsNoTracking().AnyAsync(agreement => agreement.Id == agreementId, cancellationToken))
            return NotFound(new { code = "AGREEMENT_NOT_FOUND", message = "Không tìm thấy campaign." });
        var rates = await db.SponsorshipAgreementVisitRates.AsNoTracking()
            .Where(rate => rate.SponsorshipAgreementId == agreementId)
            .OrderByDescending(rate => rate.EffectiveFromUtc)
            .ToListAsync(cancellationToken);
        return Ok(rates.Select(ToRatePeriodDto));
    }

    [HttpPost("sponsorship-agreements/{agreementId:long}/visit-rate-periods")]
    public async Task<IActionResult> AddAgreementVisitRatePeriod(long agreementId, SponsorshipAgreementVisitRateWriteRequest request, CancellationToken cancellationToken)
    {
        if (request.EffectiveFromUtc == default || !IsUsd(request.Currency) || !SponsorshipRevenuePolicy.IsValidRateInput(request.IsEnabled, request.Amount))
            return BadRequest(new { code = "VALIDATION_ERROR", message = "Kiểm tra trạng thái giá, USD/lượt và thời điểm hiệu lực." });

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var agreement = await db.SponsorshipAgreements.Include(x => x.VisitRates).FirstOrDefaultAsync(x => x.Id == agreementId, cancellationToken);
        if (agreement is null) return NotFound(new { code = "AGREEMENT_NOT_FOUND", message = "Không tìm thấy campaign." });
        if (agreement.BillingMode != SponsorshipRevenuePolicy.PerVisitBillingMode)
            return Conflict(new { code = "BILLING_MODE_CONFLICT", message = "Chỉ campaign tính theo lượt mới có lịch sử giá/lượt." });

        var requestedEffectiveFromUtc = request.EffectiveFromUtc.ToUniversalTime();
        var nowUtc = DateTime.UtcNow;
        if (requestedEffectiveFromUtc < nowUtc.AddMinutes(-1))
            return BadRequest(new { code = "RATE_BACKDATE_NOT_ALLOWED", message = "Mức giá campaign chỉ được áp dụng từ hiện tại hoặc tương lai để không diễn giải lại lịch sử." });
        var effectiveFromUtc = requestedEffectiveFromUtc < nowUtc ? nowUtc : requestedEffectiveFromUtc;
        if (effectiveFromUtc < agreement.StartsAtUtc || (agreement.EndsAtUtc is not null && effectiveFromUtc >= agreement.EndsAtUtc))
            return BadRequest(new { code = "RATE_TIME_RANGE_INVALID", message = "Thời điểm hiệu lực phải nằm trong thời hạn campaign." });
        var latestRate = agreement.VisitRates.OrderByDescending(rate => rate.EffectiveFromUtc).FirstOrDefault();
        if (latestRate is not null && effectiveFromUtc <= latestRate.EffectiveFromUtc)
            return Conflict(new { code = "RATE_ORDER_CONFLICT", message = "Thời điểm hiệu lực phải sau mức giá campaign gần nhất để giữ lịch sử không chồng chéo." });

        if (latestRate is not null) latestRate.EffectiveToUtc = effectiveFromUtc;
        var rate = new SponsorshipAgreementVisitRate
        {
            SponsorshipAgreementId = agreement.Id,
            IsEnabled = request.IsEnabled,
            Amount = request.IsEnabled ? request.Amount!.Value : 0m,
            Currency = "USD",
            EffectiveFromUtc = effectiveFromUtc
        };
        db.SponsorshipAgreementVisitRates.Add(rate);
        await db.SaveChangesAsync(cancellationToken);
        await AuditAsync("AGREEMENT_VISIT_RATE_CREATE", "SponsorshipAgreementVisitRate", rate.Id.ToString(), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Created($"/api/admin/sponsorship-agreements/{agreementId}/visit-rate-periods/{rate.Id}", ToRatePeriodDto(rate));
    }

    private async Task<ActionResult?> ValidateAdvertisementAsync(AdvertisementWriteRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || !IsHttpUrl(request.ImageUrl) || !IsHttpUrl(request.TargetUrl) || request.Placement is null || request.Placement.Trim().ToUpperInvariant() is not ("LEFT" or "RIGHT") || request.DisplayOrder < 1 || request.StartsAtUtc == default || (request.EndsAtUtc is not null && request.EndsAtUtc <= request.StartsAtUtc))
            return BadRequest(new { code = "VALIDATION_ERROR", message = "Kiểm tra tên, URL HTTPS, vị trí, thứ tự và thời gian quảng cáo." });
        if (request.SponsorId is > 0 && !await db.Sponsors.AnyAsync(x => x.Id == request.SponsorId, cancellationToken)) return BadRequest(new { code = "SPONSOR_NOT_FOUND", message = "Đối tác liên kết không tồn tại." });
        if (request.SponsorId is <= 0) return BadRequest(new { code = "VALIDATION_ERROR", message = "SponsorId phải là số dương hoặc để trống." });
        return null;
    }

    private static Advertisement BuildAdvertisement(AdvertisementWriteRequest request)
    {
        var advertisement = new Advertisement(); ApplyAdvertisement(advertisement, request); return advertisement;
    }

    private static void ApplyAdvertisement(Advertisement advertisement, AdvertisementWriteRequest request)
    {
        advertisement.SponsorId = request.SponsorId; advertisement.Name = request.Name.Trim(); advertisement.ImageUrl = request.ImageUrl.Trim(); advertisement.TargetUrl = request.TargetUrl.Trim(); advertisement.Placement = request.Placement.Trim().ToUpperInvariant(); advertisement.StartsAtUtc = request.StartsAtUtc.ToUniversalTime(); advertisement.EndsAtUtc = request.EndsAtUtc?.ToUniversalTime(); advertisement.DisplayOrder = request.DisplayOrder; advertisement.IsActive = request.IsActive;
    }

    private ActionResult? ValidateSponsorCampaign(SponsorCampaignWriteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SponsorName))
            return BadRequest(new { code = "VALIDATION_ERROR", message = "Tên nhà tài trợ là bắt buộc." });
        var error = ValidateAgreementTerms(request.CampaignName, request.BillingMode, request.CommittedAmount, request.Currency, request.Status, request.StartsAtUtc, request.EndsAtUtc, request.InitialRateEnabled, request.InitialRateAmount, request.InitialRateEffectiveFromUtc, true);
        return error is null ? null : BadRequest(new { code = "VALIDATION_ERROR", message = error });
    }

    private async Task<ActionResult?> ValidateAgreementAsync(SponsorshipAgreementWriteRequest request, bool createsInitialRate, CancellationToken cancellationToken)
    {
        if (request.SponsorId <= 0) return BadRequest(new { code = "VALIDATION_ERROR", message = "Chọn một nhà tài trợ hợp lệ." });
        if (!await db.Sponsors.AnyAsync(x => x.Id == request.SponsorId, cancellationToken)) return BadRequest(new { code = "SPONSOR_NOT_FOUND", message = "Đối tác không tồn tại." });
        var error = ValidateAgreementTerms(request.CampaignName, request.BillingMode, request.CommittedAmount, request.Currency, request.Status, request.StartsAtUtc, request.EndsAtUtc, request.InitialRateEnabled, request.InitialRateAmount, request.InitialRateEffectiveFromUtc, createsInitialRate);
        return error is null ? null : BadRequest(new { code = "VALIDATION_ERROR", message = error });
    }

    private static string? ValidateAgreementTerms(string? campaignName, string? billingMode, decimal committedAmount, string? currency, string? status, DateTime startsAtUtc, DateTime? endsAtUtc, bool initialRateEnabled, decimal? initialRateAmount, DateTime? initialRateEffectiveFromUtc, bool createsInitialRate)
    {
        var normalizedBillingMode = SponsorshipRevenuePolicy.NormalizeBillingMode(billingMode);
        var normalizedStatus = status?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(campaignName) || campaignName.Trim().Length > 160 || normalizedBillingMode is null || !IsUsd(currency) || startsAtUtc == default || (endsAtUtc is not null && endsAtUtc <= startsAtUtc) || normalizedStatus is not ("draft" or "active" or "completed" or "cancelled"))
            return "Kiểm tra tên campaign, kiểu tính tiền, USD, trạng thái và thời gian hợp đồng.";
        if (normalizedBillingMode == SponsorshipRevenuePolicy.FixedBillingMode && committedAmount <= 0m)
            return "Campaign cam kết cố định phải có giá trị USD dương.";
        if (normalizedBillingMode == SponsorshipRevenuePolicy.PerVisitBillingMode && committedAmount != 0m)
            return "Campaign tính theo lượt phải để giá trị cam kết cố định bằng 0; doanh thu được chốt theo từng lượt.";
        if (!createsInitialRate) return null;
        if (normalizedBillingMode == SponsorshipRevenuePolicy.FixedBillingMode)
            return initialRateEnabled || initialRateAmount is not null || initialRateEffectiveFromUtc is not null ? "Campaign cam kết cố định không có giá/lượt; hãy tạo campaign theo lượt riêng nếu cần." : null;
        if (initialRateEffectiveFromUtc is null || !SponsorshipRevenuePolicy.IsValidRateInput(initialRateEnabled, initialRateAmount))
            return "Campaign tính theo lượt cần trạng thái giá, USD/lượt hợp lệ và thời điểm hiệu lực.";
        var rateEffectiveFromUtc = initialRateEffectiveFromUtc.Value.ToUniversalTime();
        var agreementStartsAtUtc = startsAtUtc.ToUniversalTime();
        var agreementEndsAtUtc = endsAtUtc?.ToUniversalTime();
        return rateEffectiveFromUtc < agreementStartsAtUtc || (agreementEndsAtUtc is not null && rateEffectiveFromUtc >= agreementEndsAtUtc)
            ? "Thời điểm hiệu lực giá/lượt phải nằm trong thời hạn campaign."
            : null;
    }

    private static SponsorshipAgreement BuildAgreement(string campaignName, string billingMode, decimal committedAmount, string currency, string status, DateTime startsAtUtc, DateTime? endsAtUtc)
    {
        var agreement = new SponsorshipAgreement();
        ApplyAgreement(agreement, campaignName, billingMode, committedAmount, currency, status, startsAtUtc, endsAtUtc);
        return agreement;
    }

    private static void ApplyAgreement(SponsorshipAgreement agreement, string campaignName, string billingMode, decimal committedAmount, string currency, string status, DateTime startsAtUtc, DateTime? endsAtUtc)
    {
        agreement.CampaignName = campaignName.Trim();
        agreement.BillingMode = SponsorshipRevenuePolicy.NormalizeBillingMode(billingMode)!;
        agreement.CommittedAmount = committedAmount;
        agreement.Currency = "USD";
        agreement.Status = status.Trim().ToLowerInvariant();
        agreement.StartsAtUtc = startsAtUtc.ToUniversalTime();
        agreement.EndsAtUtc = endsAtUtc?.ToUniversalTime();
    }

    private static void AddInitialRate(SponsorshipAgreement agreement, string billingMode, bool isEnabled, decimal? amount, DateTime? effectiveFromUtc)
    {
        if (SponsorshipRevenuePolicy.NormalizeBillingMode(billingMode) != SponsorshipRevenuePolicy.PerVisitBillingMode) return;
        agreement.VisitRates.Add(new SponsorshipAgreementVisitRate
        {
            IsEnabled = isEnabled,
            Amount = isEnabled ? amount!.Value : 0m,
            Currency = "USD",
            EffectiveFromUtc = effectiveFromUtc!.Value.ToUniversalTime()
        });
    }

    private static object ToAgreementDto(SponsorshipAgreement agreement, DateTime nowUtc)
    {
        var currentRate = agreement.VisitRates
            .Where(rate => rate.EffectiveFromUtc <= nowUtc && (rate.EffectiveToUtc is null || rate.EffectiveToUtc > nowUtc))
            .OrderByDescending(rate => rate.EffectiveFromUtc)
            .FirstOrDefault();
        return new
        {
            agreement.Id,
            agreement.SponsorId,
            SponsorName = agreement.Sponsor.Name,
            agreement.CampaignName,
            agreement.BillingMode,
            agreement.CommittedAmount,
            agreement.Currency,
            agreement.Status,
            agreement.StartsAtUtc,
            agreement.EndsAtUtc,
            ReceivedAmount = agreement.Payments.Sum(payment => payment.ReceivedAmount),
            CurrentRateEnabled = currentRate?.IsEnabled == true,
            CurrentRateAmount = currentRate?.IsEnabled == true ? currentRate.Amount : (decimal?)null,
            CurrentRateCurrency = currentRate?.Currency,
            CurrentRateEffectiveFromUtc = currentRate?.EffectiveFromUtc,
            RatePeriods = agreement.VisitRates.OrderByDescending(rate => rate.EffectiveFromUtc).Select(ToRatePeriodDto).ToList()
        };
    }

    private static object ToRatePeriodDto(SponsorshipAgreementVisitRate rate) => new
    {
        rate.Id,
        rate.SponsorshipAgreementId,
        rate.IsEnabled,
        rate.Amount,
        rate.Currency,
        rate.EffectiveFromUtc,
        rate.EffectiveToUtc
    };

    private static bool IsHttpUrl(string? value) => ExternalHttpsUrlPolicy.IsValid(value);
    private static bool IsUsd(string? currency) => string.Equals(currency?.Trim(), "USD", StringComparison.OrdinalIgnoreCase);
    private static string? NormalizeLanguage(string value) => value.Trim().ToLowerInvariant() is "vi" or "en" or "ko" or "ja" or "zh" ? value.Trim().ToLowerInvariant() : null;

    private ActionResult? ValidatePoi(PoiWriteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Slug) || string.IsNullOrWhiteSpace(request.NameVi) || string.IsNullOrWhiteSpace(request.SummaryVi) || request.Latitude is < -90 or > 90 || request.Longitude is < -180 or > 180 || request.TriggerRadiusMeters <= 0)
            return BadRequest(new { code = "VALIDATION_ERROR", message = "Kiểm tra slug, tên, mô tả và tọa độ POI." });
        if (!string.IsNullOrWhiteSpace(request.ImageUrl) && !ExternalHttpsUrlPolicy.IsValid(request.ImageUrl))
            return BadRequest(new { code = "INVALID_URL", message = "Image URL phải dùng HTTPS." });
        return null;
    }

    private static bool IsValidTopic(TopicWriteRequest request) => TopicCategoryPolicy.Normalize(request.Category) is not null && !string.IsNullOrWhiteSpace(request.TitleVi) && !string.IsNullOrWhiteSpace(request.ContentVi);
    private static Poi BuildPoi(PoiWriteRequest request) => new() { Slug = request.Slug.Trim().ToLowerInvariant(), Latitude = request.Latitude, Longitude = request.Longitude, TriggerRadiusMeters = request.TriggerRadiusMeters, ImageUrl = request.ImageUrl?.Trim(), DisplayOrder = request.DisplayOrder, IsActive = request.IsActive, Translations = [new PoiTranslation { LanguageCode = "vi", Name = request.NameVi.Trim(), Summary = request.SummaryVi.Trim() }] };
    private static object ToAdminPoi(Poi poi) { var vi = poi.Translations.FirstOrDefault(x => x.LanguageCode == "vi"); return new { poi.Id, poi.Slug, poi.Latitude, poi.Longitude, poi.TriggerRadiusMeters, poi.ImageUrl, poi.DisplayOrder, poi.IsActive, NameVi = vi?.Name ?? string.Empty, SummaryVi = vi?.Summary ?? string.Empty }; }
    private static object ToAdminTopic(PoiTopic topic) { var vi = topic.Translations.FirstOrDefault(x => x.LanguageCode == "vi"); return new { topic.Id, topic.PoiId, topic.Category, topic.DisplayOrder, topic.IsActive, TitleVi = vi?.Title ?? string.Empty, ContentVi = vi?.Content ?? string.Empty }; }

    private async Task AuditAsync(string action, string entityType, string entityId, CancellationToken cancellationToken)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(id, out var adminId)) { db.AuditLogs.Add(new AuditLog { AdminUserId = adminId, Action = action, EntityType = entityType, EntityId = entityId }); await db.SaveChangesAsync(cancellationToken); }
    }
}

public static class ExternalHttpsUrlPolicy
{
    public static bool IsValid(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;
}

public static class TopicCategoryPolicy
{
    public static string? Normalize(string? category) => category?.Trim().ToLowerInvariant() is "culture" or "food" or "tourism" ? category.Trim().ToLowerInvariant() : null;
}

public enum SponsorshipPaymentValidation
{
    Accepted,
    AgreementCancelled,
    BillingModeNotEligible,
    CommitmentExceeded
}

public static class SponsorshipPaymentPolicy
{
    public static SponsorshipPaymentValidation Validate(string? status, string? billingMode, decimal committedAmount, decimal receivedAmount, decimal newPayment) =>
        string.Equals(status, "cancelled", StringComparison.OrdinalIgnoreCase)
            ? SponsorshipPaymentValidation.AgreementCancelled
            : !string.Equals(billingMode, SponsorshipRevenuePolicy.FixedBillingMode, StringComparison.OrdinalIgnoreCase)
                ? SponsorshipPaymentValidation.BillingModeNotEligible
                : WouldExceedCommitment(committedAmount, receivedAmount, newPayment)
                ? SponsorshipPaymentValidation.CommitmentExceeded
                : SponsorshipPaymentValidation.Accepted;

    public static bool WouldExceedCommitment(decimal committedAmount, decimal receivedAmount, decimal newPayment) =>
        committedAmount <= 0m || receivedAmount < 0m || newPayment <= 0m || receivedAmount + newPayment > committedAmount;

    public static bool IsCommitmentBelowReceived(decimal committedAmount, decimal receivedAmount) =>
        committedAmount <= 0m || receivedAmount < 0m || receivedAmount > committedAmount;
}
