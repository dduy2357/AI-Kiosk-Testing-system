import FormikField from '@/components/customFieldsFormik/FormikField';
import SelectField from '@/components/customFieldsFormik/SelectField';
import { Button } from '@/components/ui/button';
import { CardContent } from '@/components/ui/card';
import { ROLE_ENUM } from '@/consts/role';
import { showError } from '@/helpers/toast';
import { useAuth } from '@/providers/AuthenticationProvider';
import httpService, { SELECTED_CAMPUS_KEY } from '@/services/httpService';
import useGetListCampus from '@/services/modules/campus/hooks/useGetListCampus';
import googleloginService from '@/services/modules/googlelogin/googlelogin.service';
import { Form, Formik } from 'formik';
import { useEffect, useState, useCallback } from 'react';
import { useLocation } from 'react-router-dom';
import * as Yup from 'yup';

const LoginForm = () => {
  const { data: mockDataCampus } = useGetListCampus({
    isTrigger: true,
  });

  const [selectedCampus, setSelectedCampus] = useState(httpService.getSelectedCampus() ?? '');

  const [loading, setLoading] = useState(false);
  const { login, setIsLogging } = useAuth();
  const location = useLocation();

  useEffect(() => {
    httpService.saveSelectedCampus(selectedCampus);
  }, [selectedCampus]);

  // Function to call google-callback API
  const googleLogin = useCallback(
    async (code: string) => {
      try {
        const res = await googleloginService.loginGoogle(code, selectedCampus);
        if (res.status === 200) {
          return res.data;
        }
        throw new Error('Invalid response from google-callback');
      } catch (error) {
        showError(error);
        return null;
      }
    },
    [selectedCampus],
  );

  // Handle OAuth callback
  useEffect(() => {
    const searchParams = new URLSearchParams(location.search);
    const code = searchParams.get('code');

    if (code) {
      setLoading(true);
      googleLogin(code)
        .then((userData) => {
          if (userData?.data?.accessToken) {
            login({ data: userData.data });
            setTimeout(() => {
              if (userData.data.data?.roleId?.includes(ROLE_ENUM.Lecture)) {
                httpService.attachTokenToHeader(userData.data.accessToken);
                httpService.saveTokenStorage(userData.data.accessToken);
                setIsLogging(true);
              }
            }, 1000);

            localStorage.removeItem(SELECTED_CAMPUS_KEY);
          }
        })
        .catch((error) => {
          showError(error.message ?? 'Login failed');
        })
        .finally(() => {
          setLoading(false);
        });
    }
  }, [location.search, login, setIsLogging, selectedCampus, googleLogin]);

  // Handle Google login button click
  const handleGoogleLogin = async () => {
    try {
      setLoading(true);
      const response = await googleloginService.getLinkGoogle();
      if (response?.data?.url) {
        window.location.href = response.data.url;
      } else {
        throw new Error('No Google OAuth URL returned');
      }
    } catch (error) {
      showError('Failed to start Google login');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Formik
      initialValues={{ campus: selectedCampus }}
      validationSchema={Yup.object().shape({
        campus: Yup.string().required('Please select your campus to continue'),
      })}
      onSubmit={handleGoogleLogin}
    >
      {() => (
        <Form>
          <div className="mx-auto w-full max-w-md">
            <CardContent className="flex flex-col gap-4 rounded-xl bg-white/80 p-6 shadow-xl backdrop-blur-md">
              <p className="text-center text-sm text-blue-600">
                [Vui lòng nhập vào đây để đóng góp ý kiến, phản hồi lỗi]
              </p>

              {/* Download app button */}
              <div className="flex justify-center space-x-2">
                <Button
                  type="button"
                  variant="secondary"
                  className="bg-gray-600 text-white hover:bg-gray-700"
                  disabled={loading}
                >
                  Tải AI Kiosk Testing System
                </Button>
              </div>

              {/* Campus dropdown */}
              <div className="mt-4">
                <FormikField
                  component={SelectField}
                  name="campus"
                  options={mockDataCampus.map((campus: any) => ({
                    label: campus.name,
                    value: campus.id,
                  }))}
                  placeholder="Chọn cơ sở học"
                  shouldHideSearch
                  required
                  afterOnChange={(option: any) =>
                    setSelectedCampus(
                      Array.isArray(option) ? (option[0]?.value ?? '') : (option?.value ?? ''),
                    )
                  }
                />
              </div>

              <Button type="submit" disabled={loading}>
                Login with Google
              </Button>

              {loading && <div className="text-center text-sm text-gray-600">Đang xử lý...</div>}
            </CardContent>
          </div>
        </Form>
      )}
    </Formik>
  );
};

export default LoginForm;
