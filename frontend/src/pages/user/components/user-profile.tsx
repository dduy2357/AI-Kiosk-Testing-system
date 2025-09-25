import { User, Mail, Phone, MapPin, Calendar } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Label } from '@/components/ui/label';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import { UserDetail } from '@/services/modules/user/interfaces/userDetail.interface';
import { Button } from '@/components/ui/button';
import { t } from 'i18next';
import useToggleDialog from '@/hooks/useToggleDialog';
import DialogChangePassword, { ChangePasswordForm } from '../dialogs/DialogChangePassword';
import httpService from '@/services/httpService';
import { showError, showSuccess } from '@/helpers/toast';
import roleService from '@/services/modules/authorize/role.Service';

interface UserProfileProps {
  userProfile: UserDetail;
}

export default function UserProfileComponent({ userProfile }: UserProfileProps) {
  const userId = httpService.getUserStorage()?.userID
  const [isOpen, toggle, shouldRender] =
    useToggleDialog();
  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('vi-VN', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  };

  const getGenderText = (sex: number) => {
    return sex === 0 ? 'Nam' : 'Nữ';
  };

  const getStatusBadge = (status: number) => {
    return status === 1 ? (
      <Badge className="bg-green-100 text-green-800">Hoạt động</Badge>
    ) : (
      <Badge className="bg-red-100 text-red-800">Không hoạt động</Badge>
    );
  };

  const getInitials = (name: string) => {
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  };

  const handleChangePassword = async (values: ChangePasswordForm) => {
    if (!userId) {
      showError('Không tìm thấy người dùng, thử lại sau');
      return;
    }
    try {
      await roleService.changePassword(values);
      toggle();
      showSuccess('Đổi mật khẩu thành công');
    } catch (error) {
      showError('Đã có lỗi xảy ra, vui lòng thử lại sau');
    }
  };

  return (
    <div className="mx-auto max-w-5xl space-y-8 p-8">
      <Card>
        <CardHeader className="pb-4">
          <div className="mt-5 flex items-center justify-between">
            <CardTitle className="flex items-center gap-2">
              <User className="h-5 w-5" />
              Thông tin cá nhân
            </CardTitle>
          </div>
        </CardHeader>
        <br />
        <CardContent>
          <div className="flex flex-col gap-6 md:flex-row">
            <div className="mr-20 flex flex-col items-center space-y-4 pb-4 pt-4">
              <Avatar className="h-32 w-32">
                <AvatarImage src={userProfile.avatarUrl} alt={userProfile.fullName} />
                <AvatarFallback className="text-2xl">
                  {getInitials(userProfile.fullName)}
                </AvatarFallback>
              </Avatar>
              <div className="text-center">
                <h2 className="text-xl font-semibold">{userProfile.fullName}</h2>
                <p className="text-gray-600">{userProfile.userCode}</p>
                <p>{getStatusBadge(userProfile.status)}</p>
                <Button variant="link" className="text-gray-600" onClick={() => toggle()}>
                  {t('Navigation.ChangePassword')}
                </Button>
              </div>
            </div>

            <div className="flex-1 space-y-4">
              <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                <div className="mb-9 space-y-2">
                  <Label className="text-sm font-medium text-gray-600">Họ và tên</Label>
                  <p className="font-medium">{userProfile.fullName}</p>
                </div>

                <div className="mb-9 space-y-2">
                  <Label className="text-sm font-medium text-gray-600">Mã người dùng</Label>
                  <p className="font-medium">{userProfile.userCode}</p>
                </div>

                <div className="mb-9 space-y-2">
                  <Label className="flex items-center gap-1 text-sm font-medium text-gray-600">
                    <Mail className="h-4 w-4" />
                    Email
                  </Label>
                  <p className="font-medium">{userProfile.email}</p>
                </div>

                <div className="mb-9 space-y-2">
                  <Label className="flex items-center gap-1 text-sm font-medium text-gray-600">
                    <Phone className="h-4 w-4" />
                    Số điện thoại
                  </Label>
                  <p className="font-medium">{userProfile.phone}</p>
                </div>

                <div className="mb-9 space-y-2">
                  <Label className="flex items-center gap-1 text-sm font-medium text-gray-600">
                    <Calendar className="h-4 w-4" />
                    Ngày sinh
                  </Label>
                  <p className="font-medium">{formatDate(userProfile.dob.toString())}</p>
                </div>

                <div className="mb-9 space-y-2">
                  <Label className="text-sm font-medium text-gray-600">Giới tính</Label>
                  <p className="font-medium">{getGenderText(userProfile.sex)}</p>
                </div>
              </div>

              <div className="mb-9 space-y-2">
                <Label className="flex items-center gap-1 text-sm font-medium text-gray-600">
                  <MapPin className="h-4 w-4" />
                  Địa chỉ
                </Label>
                <p className="font-medium">{userProfile.address}</p>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {shouldRender && (
        <DialogChangePassword
          isOpen={isOpen}
          toggle={toggle}
          userId={userId}
          onSubmit={handleChangePassword}
        />
      )}
    </div>
  );
}
