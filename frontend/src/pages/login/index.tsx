import BaseUrl from '@/consts/baseUrl';
import { useAuth } from '@/providers/AuthenticationProvider';
import { Navigate } from 'react-router-dom';
import FormLogin from './components/FormLogin';
import { ImageSource } from '@/assets';
import PageWrapper from '@/components/PageWrapper/PageWrapper';
import { ROLE_ENUM } from '@/consts/role';

const Login = () => {
  const { isLogged, user } = useAuth();
  if (isLogged) {
    if (user?.roleId.includes(ROLE_ENUM.Lecture)) {
      return <Navigate to={BaseUrl.Overview} replace />;
    }

    if (user?.roleId?.includes(ROLE_ENUM.Student)) {
      return <Navigate to={BaseUrl.ExamList} replace />;
    }

    if (user?.roleId?.includes(ROLE_ENUM.SuperVisor)) {
      return <Navigate to={BaseUrl.SupervisorExamSupervision} replace />;
    }
  }

  return (
    <PageWrapper name="Đăng nhập" className="bg-white dark:bg-gray-900">
      <div
        className="relative h-screen w-screen bg-cover bg-center bg-no-repeat"
        style={{ backgroundImage: `url(${ImageSource.BackgroundLogin})` }}
      >
        <div className="absolute inset-0 bg-black/35" />{' '}
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

export default Login;
