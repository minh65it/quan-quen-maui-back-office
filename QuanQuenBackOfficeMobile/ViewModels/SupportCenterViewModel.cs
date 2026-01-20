using System.Collections.ObjectModel;

namespace QuanQuenBackOfficeMobile.ViewModels;

public class QAItem : ObservableObject
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;

    bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }
}

public partial class SupportCenterViewModel : BaseViewModel
{
	public ObservableCollection<QAItem> Items { get; } = new();

	public SupportCenterViewModel()
	{
		Seed();
	}

	void Seed()
	{
		Items.Add(new QAItem
		{
			Question = "Điều kiện để thực hiện?",
			Answer = "- Sale cài app QuanQuenBackOfficeMobile (app Agent)\n- Khách đồng ý hợp tác với Quán Quen\n- Quán có email để nhận OTP\n- CCCD/ảnh CCCD\n- Thông tin quán: họ tên người đại diện, số CCCD, số điện thoại, ảnh avatar quán, menu\n- Có thể bổ sung sau: giờ mở/đóng, ảnh/video quán, ảnh món, mô tả quán, giấy tờ (giấy phép kinh doanh, VSATTP)",
		});

		Items.Add(new QAItem
		{
			Question = "Quy trình tạo tài khoản cho quán (tóm tắt)?",
			Answer = "B1) Đăng nhập app Agent: tài khoản admin1@vsirius.com, mật khẩu test1234\nB2) Trang chủ: chọn + Tạo mới\nB3) Điền thông tin đăng ký: tên hiển thị, người đại diện (chủ/quản lý), Thành phố/Quận/Phường/Địa chỉ (địa chỉ quán), SĐT liên hệ/hotline, Email nhận OTP, loại hình kinh doanh, ảnh giấy phép (có thể dùng CCCD/giấy tờ danh tính)\nB4) Tạo mật khẩu cho quán (thường tạm thời, ví dụ: Nguyen@123)\nB5) Nhập OTP được gửi về email (kiểm tra hộp thư rác nếu cần)\nB6) Ký thỏa thuận/hợp đồng điện tử (so khớp chữ ký, không khớp thì ký lại)\nB7) Cập nhật ảnh avatar cho quán (ảnh quán cung cấp hoặc chụp tại chỗ)\nB8) Cập nhật ảnh menu, video (có thể bỏ qua để làm sau)\nB9) Chọn trạng thái Hoạt động; cập nhật tiện ích và loại hình phù hợp thực tế\nB10) Hoàn tất; sale dùng tài khoản để cập nhật thông tin cơ bản và tạo menu điện tử, sau đó bàn giao tài khoản/mật khẩu cho quán",
		});

		Items.Add(new QAItem
		{
			Question = "Vừa qua bước đặt mật khẩu, đã gửi OTP nhưng chưa nhập (email \"bị giữ\")?",
			Answer = "Hệ thống đang giữ email, không thể tiếp tục tạo mới; tài khoản thực tế chưa hình thành. Cách xử lý: liên hệ IT xoá email đó khỏi hệ thống rồi thực hiện lại quy trình tạo tài khoản.",
		});

		Items.Add(new QAItem
		{
			Question = "Đã nhập OTP nhưng chưa đi đến bước cuối?",
			Answer = "Ngay khi nhập OTP thành công, tài khoản đã được tạo. Có thể duyệt thủ công trên DMS để kích hoạt rồi bổ sung thông tin trên App/Web sau. Cách duyệt: đăng nhập dms.quanquen.com (admin1@vsirius.com / test1234) → Manage Store → Store → tìm quán → cấp quyền: Level 3, gói Quán Quen, trạng thái Active.",
		});

		Items.Add(new QAItem
		{
			Question = "Khách tự đăng ký trên app Quán Quen Store thì duyệt ở đâu?",
			Answer = "Đăng nhập dms.quanquen.com (admin1@vsirius.com / test1234) → Manage Store → Store → tìm tên quán vừa tạo → cấp quyền: Level 3, gói Quán Quen, trạng thái Active.",
		});
	}
}


