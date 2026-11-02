# E1 — nội dung và ngôn ngữ

Ngày kiểm tra tự động: 02/11/2026. Nguồn: main `a39128650e5e780dc4a2fded45a74b81a50f1c1e`. Bản gói ban đầu: `0cd83f1`.

## Kết quả đã thực hiện

Đọc JSON UTF-8 tại workspace và kiểm tra từng POI/chủ đề cho vi/ko/ja/zh/en:

| Kiểm tra | Thực tế |
| --- | --- |
| POI | 11 |
| Chủ đề | 33 |
| Bản ghi bản dịch POI + chủ đề | 220 (55 + 165) |
| Thiếu bản dịch trong năm ngôn ngữ | 0 |
| Name/summary hoặc title/content rỗng | 0 |

Đây là kiểm tra độ phủ/cấu trúc, không chứng minh dịch đúng nghĩa. Bộ test C# được giữ nguyên từ main nhưng chưa chạy trong gói không có csproj; không ghi PASS C# cho lần bàn giao này.

## Rà soát chuyên môn cần làm

Với từng slug và category culture/food/tourism: đối chiếu tên riêng, sự kiện, nghĩa bản dịch và khả năng hiểu; kiểm tra sourceUrls của POI với từng tuyên bố liên quan. Có URL trong catalog không chứng minh mọi câu đã được xác minh hoặc nguồn còn truy cập được. Cần người đọc hiểu từng ngôn ngữ đánh giá; không suy ra chất lượng từ độ phủ.

| Ngôn ngữ | Độ phủ cấu trúc | Nghĩa/tên riêng/phát âm |
| --- | --- | --- |
| vi | Đủ | Chưa nghiệm thu thủ công |
| ko | Đủ | Chưa nghiệm thu thủ công |
| ja | Đủ | Chưa nghiệm thu thủ công |
| zh | Đủ | Chưa nghiệm thu thủ công |
| en | Đủ | Chưa nghiệm thu thủ công |

## Mẫu ghi lỗi thật

| Ngày/người rà | Commit | Slug/chủ đề/ngôn ngữ | Câu gốc | Vấn đề và nguồn đối chiếu | Đề xuất sửa | Người xác nhận |
| --- | --- | --- | --- | --- | --- | --- |
| Chưa đo | — | — | — | — | — | — |

Không tạo ví dụ lỗi hoặc tỷ lệ chất lượng khi chưa thực sự rà soát. Chưa chỉnh lời dịch vì yêu cầu gói này chỉ lấy code từ main.
