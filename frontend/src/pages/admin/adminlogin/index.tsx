import { ImageSource } from '@/assets';

import BaseUrl from '@/consts/baseUrl';
import { useAuth } from '@/providers/AuthenticationProvider';
import { Navigate } from 'react-router-dom';
import FormLogin from './components/form-login';
import PageWrapper from '@/components/PageWrapper/PageWrapper';
import { ROLE_ENUM } from '@/consts/role';

const AdminLoginPage = () => {
  const { isLogged, user } = useAuth();

  if (isLogged && user?.roleId?.includes(ROLE_ENUM.ADMIN)) {
    return <Navigate to={BaseUrl.AdminManageUsers} replace />;
  }

  return (
    <PageWrapper name="Đăng nhập quản trị viên" className="bg-white dark:bg-gray-900">
      <div
        className="relative h-screen w-screen overflow-hidden"
        style={{
          backgroundImage: `url(${ImageSource.BackgroundLogin})`,
          backgroundSize: 'cover',
          backgroundPosition: 'center',
        }}
      >
        <div className="absolute inset-0 bg-black/50" /> {/* Overlay for better text visibility */}
        <div className="relative z-10 flex h-full flex-col items-center justify-center">
          <img
            loading="lazy"
            src={ImageSource.LogoFPT ?? ''}
            alt="Logo"
            className="mb-8 h-32 rounded-md"
          />
          <FormLogin />
        </div>
      </div>
    </PageWrapper>
  );
};

export default AdminLoginPage;
