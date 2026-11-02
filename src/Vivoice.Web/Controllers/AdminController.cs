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
[Authorize(Roles = "admin")]
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
        poi.Slug = request.Slug.Trim().ToLowerInvariant(); poi.Latitude = request.Latitude; poi.Longitude = request.Longitude; poi.ImageUrl = request.ImageUrl?.Trim(); poi.DisplayOrder = request.DisplayOrder; poi.IsActive = request.IsActive; poi.UpdatedAtUtc = DateTime.UtcNow;
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
            ContractedAmount = x.Agreements.Where(a => a.Status != "cancelled").Sum(a => (decimal?)a.CommittedAmount) ?? 0m
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
        var agreements = await db.SponsorshipAgreements.AsNoTracking().Include(x => x.Sponsor).Include(x => x.Payments).OrderByDescending(x => x.StartsAtUtc)
            .Select(x => new { x.Id, x.SponsorId, SponsorName = x.Sponsor.Name, x.CommittedAmount, x.Currency, x.Status, x.StartsAtUtc, x.EndsAtUtc, ReceivedAmount = x.Payments.Sum(p => (decimal?)p.ReceivedAmount) ?? 0m })
            .ToListAsync(cancellationToken);
        return Ok(agreements);
    }

    [HttpPost("sponsorship-agreements")]
    public async Task<IActionResult> CreateAgreement(SponsorshipAgreementWriteRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateAgreementAsync(request, cancellationToken); if (validation is not null) return validation;
        var agreement = BuildAgreement(request); db.SponsorshipAgreements.Add(agreement); await db.SaveChangesAsync(cancellationToken); await AuditAsync("AGREEMENT_CREATE", "SponsorshipAgreement", agreement.Id.ToString(), cancellationToken);
        return Created($"/api/admin/sponsorship-agreements/{agreement.Id}", new { agreement.Id });
    }

    [HttpPatch("sponsorship-agreements/{agreementId:long}")]
    public async Task<IActionResult> UpdateAgreement(long agreementId, SponsorshipAgreementWriteRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateAgreementAsync(request, cancellationToken); if (validation is not null) return validation;
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var agreement = await db.SponsorshipAgreements.Include(x => x.Payments).FirstOrDefaultAsync(x => x.Id == agreementId, cancellationToken);
        if (agreement is null) return NotFound(new { code = "AGREEMENT_NOT_FOUND", message = "Không tìm thấy hợp đồng." });
        if (SponsorshipPaymentPolicy.IsCommitmentBelowReceived(request.CommittedAmount, agreement.Payments.Sum(payment => payment.ReceivedAmount)))
            return Conflict(new { code = "COMMITMENT_CONFLICT", message = "Giá trị hợp đồng không thể thấp hơn tổng khoản đã nhận." });
        ApplyAgreement(agreement, request); await db.SaveChangesAsync(cancellationToken); await AuditAsync("AGREEMENT_UPDATE", "SponsorshipAgreement", agreement.Id.ToString(), cancellationToken); await transaction.CommitAsync(cancellationToken);
        return Ok(new { agreement.Id });
    }

    [HttpPost("sponsorship-agreements/{agreementId:long}/payments")]
    public async Task<IActionResult> AddPayment(long agreementId, SponsorshipPaymentWriteRequest request, CancellationToken cancellationToken)
    {
        if (request.ReceivedAmount <= 0 || request.ReceivedAtUtc == default) return BadRequest(new { code = "VALIDATION_ERROR", message = "Số tiền nhận và thời điểm nhận phải hợp lệ." });
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var agreement = await db.SponsorshipAgreements.Include(x => x.Payments).FirstOrDefaultAsync(x => x.Id == agreementId, cancellationToken);
        if (agreement is null) return NotFound(new { code = "AGREEMENT_NOT_FOUND", message = "Không tìm thấy hợp đồng." });
        var validation = SponsorshipPaymentPolicy.Validate(agreement.Status, agreement.CommittedAmount, agreement.Payments.Sum(payment => payment.ReceivedAmount), request.ReceivedAmount);
        if (validation == SponsorshipPaymentValidation.AgreementCancelled)
            return Conflict(new { code = "AGREEMENT_CANCELLED", message = "Không thể ghi nhận thanh toán cho hợp đồng đã hủy." });
        if (validation == SponsorshipPaymentValidation.CommitmentExceeded)
            return Conflict(new { code = "PAYMENT_EXCEEDS_COMMITMENT", message = "Khoản thanh toán vượt giá trị hợp đồng còn lại." });
        var payment = new SponsorshipPayment { SponsorshipAgreementId = agreementId, ReceivedAmount = request.ReceivedAmount, ReceivedAtUtc = request.ReceivedAtUtc.ToUniversalTime() };
        db.SponsorshipPayments.Add(payment); await db.SaveChangesAsync(cancellationToken); await AuditAsync("PAYMENT_CREATE", "SponsorshipPayment", payment.Id.ToString(), cancellationToken); await transaction.CommitAsync(cancellationToken);
        return Created($"/api/admin/sponsorship-agreements/{agreementId}/payments/{payment.Id}", new { payment.Id });
    }

    [HttpGet("revenue-rates")]
    public async Task<IActionResult> GetRevenueRates(CancellationToken cancellationToken)
    {
        var rates = await db.VisitRevenueRates.AsNoTracking().OrderByDescending(x => x.EffectiveFromUtc)
            .Select(x => new { x.Id, x.Amount, x.Currency, x.EffectiveFromUtc, x.EffectiveToUtc }).ToListAsync(cancellationToken);
        return Ok(rates);
    }

    [HttpPost("revenue-rates")]
    public async Task<IActionResult> CreateRevenueRate(RevenueRateWriteRequest request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0 || !IsUsd(request.Currency) || request.EffectiveFromUtc == default) return BadRequest(new { code = "VALIDATION_ERROR", message = "VIVOICE hiện tổng hợp dashboard bằng USD; nhập mức giá dương và thời điểm hiệu lực hợp lệ." });
        var effectiveFrom = request.EffectiveFromUtc.ToUniversalTime();
        var latest = await db.VisitRevenueRates.OrderByDescending(x => x.EffectiveFromUtc).FirstOrDefaultAsync(cancellationToken);
        if (latest is not null && effectiveFrom <= latest.EffectiveFromUtc) return Conflict(new { code = "RATE_ORDER_CONFLICT", message = "Thời điểm hiệu lực phải sau mức giá gần nhất để giữ lịch sử không chồng chéo." });
        if (latest is not null) latest.EffectiveToUtc = effectiveFrom;
        var rate = new VisitRevenueRate { Amount = request.Amount, Currency = "USD", EffectiveFromUtc = effectiveFrom };
        db.VisitRevenueRates.Add(rate); await db.SaveChangesAsync(cancellationToken); await AuditAsync("REVENUE_RATE_CREATE", "VisitRevenueRate", rate.Id.ToString(), cancellationToken);
        return Created($"/api/admin/revenue-rates/{rate.Id}", new { rate.Id });
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

    private async Task<ActionResult?> ValidateAgreementAsync(SponsorshipAgreementWriteRequest request, CancellationToken cancellationToken)
    {
        var normalizedStatus = request.Status?.Trim().ToLowerInvariant();
        if (request.SponsorId <= 0 || request.CommittedAmount <= 0 || !IsUsd(request.Currency) || request.StartsAtUtc == default || (request.EndsAtUtc is not null && request.EndsAtUtc <= request.StartsAtUtc) || normalizedStatus is not ("draft" or "active" or "completed" or "cancelled"))
            return BadRequest(new { code = "VALIDATION_ERROR", message = "Kiểm tra đối tác, số tiền USD, trạng thái và thời gian hợp đồng." });
        if (!await db.Sponsors.AnyAsync(x => x.Id == request.SponsorId, cancellationToken)) return BadRequest(new { code = "SPONSOR_NOT_FOUND", message = "Đối tác không tồn tại." });
        return null;
    }

    private static SponsorshipAgreement BuildAgreement(SponsorshipAgreementWriteRequest request)
    {
        var agreement = new SponsorshipAgreement(); ApplyAgreement(agreement, request); return agreement;
    }

    private static void ApplyAgreement(SponsorshipAgreement agreement, SponsorshipAgreementWriteRequest request)
    {
        agreement.SponsorId = request.SponsorId; agreement.CommittedAmount = request.CommittedAmount; agreement.Currency = "USD"; agreement.Status = request.Status.Trim().ToLowerInvariant(); agreement.StartsAtUtc = request.StartsAtUtc.ToUniversalTime(); agreement.EndsAtUtc = request.EndsAtUtc?.ToUniversalTime();
    }

    private static bool IsHttpUrl(string? value) => ExternalHttpsUrlPolicy.IsValid(value);
    private static bool IsUsd(string? currency) => string.Equals(currency?.Trim(), "USD", StringComparison.OrdinalIgnoreCase);
    private static string? NormalizeLanguage(string value) => value.Trim().ToLowerInvariant() is "vi" or "ko" or "ja" or "zh" or "th" ? value.Trim().ToLowerInvariant() : null;

    private ActionResult? ValidatePoi(PoiWriteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Slug) || string.IsNullOrWhiteSpace(request.NameVi) || string.IsNullOrWhiteSpace(request.SummaryVi) || request.Latitude is < -90 or > 90 || request.Longitude is < -180 or > 180)
            return BadRequest(new { code = "VALIDATION_ERROR", message = "Kiểm tra slug, tên, mô tả và tọa độ POI." });
        if (!string.IsNullOrWhiteSpace(request.ImageUrl) && !ExternalHttpsUrlPolicy.IsValid(request.ImageUrl))
            return BadRequest(new { code = "INVALID_URL", message = "Image URL phải dùng HTTPS." });
        return null;
    }

    private static bool IsValidTopic(TopicWriteRequest request) => TopicCategoryPolicy.Normalize(request.Category) is not null && !string.IsNullOrWhiteSpace(request.TitleVi) && !string.IsNullOrWhiteSpace(request.ContentVi);
    private static Poi BuildPoi(PoiWriteRequest request) => new() { Slug = request.Slug.Trim().ToLowerInvariant(), Latitude = request.Latitude, Longitude = request.Longitude, ImageUrl = request.ImageUrl?.Trim(), DisplayOrder = request.DisplayOrder, IsActive = request.IsActive, Translations = [new PoiTranslation { LanguageCode = "vi", Name = request.NameVi.Trim(), Summary = request.SummaryVi.Trim() }] };
    private static object ToAdminPoi(Poi poi) { var vi = poi.Translations.FirstOrDefault(x => x.LanguageCode == "vi"); return new { poi.Id, poi.Slug, poi.Latitude, poi.Longitude, poi.ImageUrl, poi.DisplayOrder, poi.IsActive, NameVi = vi?.Name ?? string.Empty, SummaryVi = vi?.Summary ?? string.Empty }; }
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
    CommitmentExceeded
}

public static class SponsorshipPaymentPolicy
{
    public static SponsorshipPaymentValidation Validate(string? status, decimal committedAmount, decimal receivedAmount, decimal newPayment) =>
        string.Equals(status, "cancelled", StringComparison.OrdinalIgnoreCase)
            ? SponsorshipPaymentValidation.AgreementCancelled
            : WouldExceedCommitment(committedAmount, receivedAmount, newPayment)
                ? SponsorshipPaymentValidation.CommitmentExceeded
                : SponsorshipPaymentValidation.Accepted;

    public static bool WouldExceedCommitment(decimal committedAmount, decimal receivedAmount, decimal newPayment) =>
        committedAmount <= 0m || receivedAmount < 0m || newPayment <= 0m || receivedAmount + newPayment > committedAmount;

    public static bool IsCommitmentBelowReceived(decimal committedAmount, decimal receivedAmount) =>
        committedAmount <= 0m || receivedAmount < 0m || receivedAmount > committedAmount;
}
