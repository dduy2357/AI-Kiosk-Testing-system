import { FormRegister } from "./components/FormRegister";

const RegisterPage = () => {
  return (
    <div className="to-primary-dark/20 relative min-h-screen w-full overflow-hidden bg-gradient-to-r from-primary/20">
      <div className="absolute inset-0">
        {/* <img
          src={ImageSource.imgBgRegister || "/placeholder.svg"}
          className="h-full w-full object-cover opacity-50"
          alt="Background"
        /> */}
      </div>
      <div className="relative z-10 flex min-h-screen flex-col items-center justify-center p-4">
        {/* <img
          src={ImageSource.logoApp || "/placeholder.svg"}
          alt="Logo"
          className="mb-8 h-32 w-32 rounded-md"
        /> */}
        <FormRegister />
      </div>
    </div>
  );
};

export default RegisterPage;
