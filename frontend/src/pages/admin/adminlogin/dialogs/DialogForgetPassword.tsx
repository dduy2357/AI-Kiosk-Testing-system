import FormikField from "@/components/customFieldsFormik/FormikField";
import InputField from "@/components/customFieldsFormik/InputField";
import { Button } from "@/components/ui/button";
import {
    Dialog,
    DialogPortal,
    DialogOverlay,
    DialogContent,
    DialogTitle,
    DialogDescription,
} from "@/components/ui/dialog";
import { DialogI } from "@/interfaces/common";
import { Form, Formik } from "formik";
import { Fragment } from "react";
import { useTranslation } from "react-i18next";

export interface ForgetPasswordForm {
    email: string
}
interface DialogProps extends DialogI<any> {
    onSubmit?: (values: ForgetPasswordForm) => void;
}

const DialogForgetPassword = (props: DialogProps) => {
    const { isOpen, toggle, onSubmit } = props;
    const { t } = useTranslation("shared");

    const initialValues = {
        email: '',
    };

    return (
        <Dialog open={isOpen} onOpenChange={toggle}>
            <DialogPortal>
                <DialogOverlay />
                <DialogContent className="max-h-[80vh] overflow-y-auto">
                    <Formik
                        initialValues={initialValues}
                        onSubmit={onSubmit || (() => { })}>
                        {({ isSubmitting }) => {
                            return (
                                <Fragment>
                                    <DialogTitle>Quên mật khẩu</DialogTitle>
                                    <DialogDescription>Vui lòng nhập email, chúng tôi sẽ gửi mật khẩu mới đến email của bạn</DialogDescription>
                                    <Form className="mt-[25px]  justify-end gap-2">
                                        <div className="space-y-2">
                                            <FormikField
                                                id="email"
                                                component={InputField}
                                                name="email"
                                                label='Email'
                                                placeholder='Email'
                                                required
                                                type="email"
                                                className="transition-all duration-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20"
                                            />
                                        </div>
                                        <br />
                                        <div className="flex justify-end gap-2">
                                            <Button type="submit" isLoading={isSubmitting}>
                                                Gửi
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

export default DialogForgetPassword;
