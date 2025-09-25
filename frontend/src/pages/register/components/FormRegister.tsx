import FormikField from "@/components/customFieldsFormik/FormikField";
import InputField from "@/components/customFieldsFormik/InputField";
import { Button } from "@/components/ui/button";
import { CardContent } from "@/components/ui/card";
import { Progress } from "@/components/ui/progress";
import BaseUrl from "@/consts/baseUrl";
import { regexCommon } from "@/consts/regex";
import { sleepTime } from "@/helpers/common";
import { showError, showSuccess } from "@/helpers/toast";
import { Form, Formik } from "formik";
import { useNavigate } from "react-router-dom";
import * as Yup from "yup";

const validationSchema = Yup.object().shape({
  Password: Yup.string()
    .required("Mật khẩu không được để trống.")
    .matches(
      regexCommon.regexStrongPassword,
      "Mật khẩu phải chứa ít nhất 8 ký tự, 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt",
    ),
  RePassword: Yup.string()
    .required("Mật khẩu xác nhận không được để trống.")
    .oneOf([Yup.ref("Password")], "Mật khẩu không khớp"),
  FullName: Yup.string()
    .required("Tên đầy đủ không được để trống.")
    .min(10, "Tên đầy đủ phải chứa ít nhất 10 ký tự"),
  PhoneNumber: Yup.string()
    .required("Số điện thoại không được để trống.")
    .matches(regexCommon.regexPhone, "Số điện thoại không đúng định dạng"),
  Email: Yup.string()
    .required("Email không được để trống.")
    .email("Email không đúng định dạng."),
  DetailAddress: Yup.string().required("Địa chỉ không được để trống."),
  ProvinceID: Yup.number().required("Tỉnh/Thành Phố không được để trống."),
  DistrictID: Yup.number().required("Quận/Huyện không được để trống."),
  WardID: Yup.number().required("Xã/Phường không được để trống."),
});

export const FormRegister = () => {
  const navigate = useNavigate();

  return (
    <Formik
      validationSchema={validationSchema}
      initialValues={{
        Password: "",
        RePassword: "",
        FullName: "",
        PhoneNumber: "",
        Email: "",
        DetailAddress: "",
        ProvinceID: 0,
        DistrictID: 0,
        WardID: 0,
      }}
      onSubmit={async (_values, { setSubmitting }) => {
        try {
          setSubmitting(true);
          await sleepTime(1000);
          // const payload = {
          //   FullName: values.FullName,
          //   PhoneNumber: values.PhoneNumber,
          //   Email: values.Email,
          //   Password: values.Password,
          //   RePassword: values.RePassword,
          //   DetailAddress: values.DetailAddress,
          //   ProvinceID: values.ProvinceID,
          //   DistrictID: values.DistrictID,
          //   WardID: values.WardID,
          // };
          //   await registerService.register(payload);
          navigate(BaseUrl.Login);
          showSuccess("Đăng ký tài khoản mới thành công");
        } catch (error) {
          showError(error);
        } finally {
          setSubmitting(false);
        }
      }}
    >
      {({ values, isSubmitting }) => {
        const passwordStrength =
          values.Password.length > 0
            ? values.Password.match(regexCommon.regexStrongPassword)
              ? 100
              : values.Password.length >= 8
                ? 50
                : 25
            : 0;

        return (
          <Form className="w-full max-w-md space-y-6">
            <CardContent className="rounded-xl bg-white/80 p-6 shadow-xl backdrop-blur-md">
              <h2 className="mb-6 text-center text-3xl font-bold text-gray-800">
                Đăng ký
              </h2>
              <div className="space-y-4">
                <FormikField
                  component={InputField}
                  name="FullName"
                  label="Tên đầy đủ"
                  placeholder="Nhập tên đầy đủ của bạn"
                  required
                />
                <FormikField
                  component={InputField}
                  name="Email"
                  label="Email"
                  placeholder="Nhập email của bạn"
                  required
                />
                <FormikField
                  component={InputField}
                  name="PhoneNumber"
                  label="Số điện thoại"
                  placeholder="Nhập số điện thoại của bạn"
                  required
                />
                <FormikField
                  component={InputField}
                  name="Password"
                  type="password"
                  label="Mật khẩu"
                  placeholder="Nhập mật khẩu của bạn"
                  required
                />
                <Progress value={passwordStrength} className="w-full" />
                <FormikField
                  component={InputField}
                  name="RePassword"
                  type="password"
                  label="Xác nhận mật khẩu"
                  placeholder="Nhập lại mật khẩu của bạn"
                  required
                />
                <FormikField
                  component={InputField}
                  name="DetailAddress"
                  label="Địa chỉ"
                  placeholder="Nhập địa chỉ của bạn"
                  required
                />

                <Button
                  className="w-full bg-primary hover:bg-primary/90"
                  type="submit"
                  disabled={isSubmitting}
                >
                  {isSubmitting ? "Đang xử lý..." : "Đăng ký"}
                </Button>

                <div className="text-center text-sm">
                  Đã có tài khoản?{" "}
                  <Button
                    variant="link"
                    className="text-primary hover:underline"
                    onClick={() => navigate(BaseUrl.Login)}
                  >
                    Đăng nhập
                  </Button>
                </div>
              </div>
            </CardContent>
          </Form>
        );
      }}
    </Formik>
  );
};
