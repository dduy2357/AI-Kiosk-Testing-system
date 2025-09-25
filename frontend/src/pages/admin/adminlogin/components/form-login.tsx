import { regexCommon } from '@/components/consts/regex';
import FormikField from '@/components/customFieldsFormik/FormikField';
import InputField from '@/components/customFieldsFormik/InputField';
import SelectField from '@/components/customFieldsFormik/SelectField';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Label } from '@/components/ui/label';
import { useToast } from '@/components/ui/use-toast';
import BaseUrl from '@/consts/baseUrl';
import { ROLE_ENUM } from '@/consts/role';
import { sleepTime } from '@/helpers/common';
import useToggleDialog from '@/hooks/useToggleDialog';
import { UserInfo } from '@/interfaces/user';
import { useAuth } from '@/providers/AuthenticationProvider';
import useGetListCampus from '@/services/modules/campus/hooks/useGetListCampus';
import { Form, Formik } from 'formik';
import { Loader2, Lock } from 'lucide-react';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import * as Yup from 'yup';
import DialogForgetPassword, { ForgetPasswordForm } from '../dialogs/DialogForgetPassword';
import roleService from '@/services/modules/authorize/role.Service';
import { showError, showSuccess } from '@/helpers/toast';

const FormLogin = () => {
  const { toast } = useToast();
  const { loginWithAccount } = useAuth();
  const [showPassword] = useState(false);
  const navigate = useNavigate();
  const [isOpen, toggle, shouldRender] = useToggleDialog();
  const { data: mockDataCampus } = useGetListCampus({
    isTrigger: true,
  });

  const handleForgetPassword = async (values: ForgetPasswordForm) => {
    try {
      const response = await roleService.forgetPassword(values);

      toggle();
      showSuccess(response.data.message);
    } catch (error) {
      showError(error);
    }
  };
  return (
    <>
      <Formik
        validationSchema={Yup.object().shape({
          username: Yup.string()
            .required('Email không được để trống')
            .email('Email không đúng định dạng'),
          password: Yup.string()
            .required('Mật khẩu không được để trống')
            .matches(
              regexCommon.regexStrongPassword,
              'Mật khẩu phải chứa ít nhất 8 ký tự, 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt',
            ),
          campus: Yup.string().required('Please select your campus to continue'),
        })}
        initialValues={{
          username: '',
          password: '',
          campus: '',
        }}
        onSubmit={async (values, { setSubmitting }) => {
          try {
            setSubmitting(true);
            const { username, password } = values;
            await sleepTime(2000);
            const user = (await loginWithAccount({
              username,
              password,
            })) as unknown as UserInfo | null;
            if (user?.roleId?.includes(ROLE_ENUM.ADMIN)) {
              navigate(BaseUrl.AdminManageUsers);
            }
          } catch (error) {
            toast({
              variant: 'destructive',
              description: error as string,
            });
          } finally {
            setSubmitting(false);
          }
        }}
      >
        {({ values, handleChange, handleBlur, isSubmitting }) => (
          <Form className="space-y-6">
            <Card className="border-0 bg-white/95 shadow-2xl backdrop-blur-xl">
              <CardContent className="p-8">
                <div className="space-y-6">
                  <div className="text-center">
                    <h2 className="text-2xl font-bold text-gray-800">Đăng nhập</h2>
                    <p className="mt-2 text-sm text-gray-600">
                      Vui lòng nhập thông tin đăng nhập của bạn
                    </p>
                  </div>

                  <div className="space-y-2">
                    <FormikField
                      component={SelectField}
                      name="campus"
                      label="Chọn cơ sở"
                      options={mockDataCampus.map((campus: any) => ({
                        label: campus.name,
                        value: campus.id,
                      }))}
                      placeholder="Chọn cơ sở học"
                      shouldHideSearch
                      required
                    />
                  </div>

                  <div className="space-y-2">
                    <FormikField
                      component={InputField}
                      name="username"
                      label="Email"
                      placeholder="Nhập email của bạn"
                      type="email"
                      value={values.username}
                      onChange={handleChange}
                      onBlur={handleBlur}
                      required
                    />
                  </div>

                  {/* Password Field */}
                  <div className="space-y-2">
                    <Label
                      htmlFor="password"
                      className="flex items-center gap-2 text-sm font-medium text-gray-700"
                    >
                      <Lock className="h-4 w-4" />
                      Mật khẩu
                    </Label>
                    <div className="relative">
                      <FormikField
                        component={InputField}
                        name="password"
                        type={showPassword ? 'text' : 'password'}
                        placeholder="Nhập mật khẩu của bạn"
                        value={values.password}
                        onChange={handleChange}
                        onBlur={handleBlur}
                        className="pr-10"
                        required
                      />
                    </div>
                  </div>

                  {/* Login Button */}
                  <Button
                    type="submit"
                    disabled={isSubmitting}
                    className="h-12 w-full bg-gradient-to-r from-blue-600 to-purple-600 text-white transition-all duration-200 hover:from-blue-700 hover:to-purple-700 hover:shadow-lg disabled:opacity-50"
                  >
                    {isSubmitting ? (
                      <div className="flex items-center gap-2">
                        <Loader2 className="h-4 w-4 animate-spin" />
                        Đang xử lý...
                      </div>
                    ) : (
                      'Đăng nhập'
                    )}
                  </Button>
                  <div className="flex justify-end" style={{ marginTop: '0px' }}>
                    <Button variant="link" onClick={() => toggle()}>
                      Quên mật khẩu?
                    </Button>
                  </div>
                  <div className="space-y-3">
                    <p className="text-center text-xs text-gray-500">
                      Bằng cách đăng nhập, bạn đồng ý với{' '}
                      <button
                        type="button"
                        className="cursor-pointer border-none bg-transparent p-0 text-blue-600 hover:underline"
                      >
                        Điều khoản sử dụng
                      </button>{' '}
                      và{' '}
                      <button
                        type="button"
                        className="cursor-pointer border-none bg-transparent p-0 text-blue-600 hover:underline"
                      >
                        Chính sách bảo mật
                      </button>
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>
          </Form>
        )}
      </Formik>
      {shouldRender && (
        <DialogForgetPassword isOpen={isOpen} toggle={toggle} onSubmit={handleForgetPassword} />
      )}
    </>
  );
};

export default FormLogin;
