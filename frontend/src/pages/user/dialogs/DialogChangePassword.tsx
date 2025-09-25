import { regexCommon } from "@/components/consts/regex";
import FormikField from "@/components/customFieldsFormik/FormikField";
import InputField from "@/components/customFieldsFormik/InputField";
import { Button } from "@/components/ui/button";
import {
    Dialog,
    DialogPortal,
    DialogOverlay,
    DialogContent,
    DialogTitle,
} from "@/components/ui/dialog";
import { DialogI } from "@/interfaces/common";
import { Form, Formik } from "formik";
import { Fragment } from "react";
import { useTranslation } from "react-i18next";
import * as Yup from 'yup';

interface DialogProps extends DialogI<any> {
    userId?: string;
    onSubmit?: (values: ChangePasswordForm) => void;
}

export interface ChangePasswordForm {
    userId: string;
    exPassword: string;
    password: string;
    rePassword: string;
}
const DialogChangePassword = (props: DialogProps) => {
    const { isOpen, toggle, onSubmit } = props;
    const { t } = useTranslation("shared");

    const initialValues: ChangePasswordForm = {
        userId: props.userId || '',
        exPassword: '',
        password: '',
        rePassword: '',
    };

    const validationSchema = Yup.object({
        password: Yup.string()
            .required('Mật khẩu không được để trống')
            .matches(
                regexCommon.regexStrongPassword,
                'Mật khẩu phải chứa ít nhất 8 ký tự, 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt',
            ),
        rePassword: Yup.string()
            .required('Xác nhận mật khẩu không được để trống')
            .test('password-match', 'Mật khẩu xác nhận phải khớp với mật khẩu', function (value) {
                return value === this.parent.password;
            })
            .matches(
                regexCommon.regexStrongPassword,
                'Mật khẩu xác nhận phải chứa ít nhất 8 ký tự, 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt',
            ),
    });

    return (
        <Dialog open={isOpen} onOpenChange={toggle}>
            <DialogPortal>
                <DialogOverlay />
                <DialogContent className="max-h-[80vh] overflow-y-auto">
                    <Formik
                        initialValues={initialValues}
                        validationSchema={validationSchema}
                        onSubmit={onSubmit || (() => { })}>
                        {({ isSubmitting }) => {
                            return (
                                <Fragment>
                                    <DialogTitle>{t('Navigation.ChangePassword')}</DialogTitle>
                                    <Form className="mt-[25px]  justify-end gap-2">
                                        <div className="space-y-2">
                                            <FormikField
                                                id="exPassword"
                                                component={InputField}
                                                name="exPassword"
                                                label='Mật khẩu hiện tại'
                                                placeholder='Mật khẩu hiện tại'
                                                required
                                                type="password"
                                                className="transition-all duration-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20"
                                            />
                                            <FormikField
                                                id="password"
                                                component={InputField}
                                                name="password"
                                                label='Mật khẩu mới'
                                                placeholder='Mật khẩu mới'
                                                required
                                                type="password"
                                                className="transition-all duration-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20"
                                            />
                                            <FormikField
                                                id="rePassword"
                                                component={InputField}
                                                name="rePassword"
                                                label='Viết lại mật khẩu mới'
                                                placeholder='Viết lại mật khẩu mới'
                                                required
                                                type="password"
                                                className="transition-all duration-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20"
                                            />
                                        </div>
                                        <br />
                                        <div className="flex justify-end gap-2">
                                            <Button type="submit" isLoading={isSubmitting}>
                                                {t("Navigation.Change")}
                                            </Button>
                                            <Button variant="ghost" type="button" onClick={toggle}>
                                                {t("Close")}
                                            </Button>
                                        </div>
                                    </Form>
                                </Fragment>
                            );
                        }}
                    </Formik>
                </DialogContent>
            </DialogPortal>
        </Dialog>
    );
};

export default DialogChangePassword;
