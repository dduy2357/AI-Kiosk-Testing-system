import type React from "react";
import Loading from "../ui/loading";

interface IPageWrapper {
  name: string;
  className?: string;
  children: React.ReactNode;
  isLoading?: boolean;
}

const PageWrapper = (props: IPageWrapper) => {
  const { name, className, children, isLoading = false } = props;

  return (
    <div
      className={`component:${name} page-full relative flex flex-col ${className}`}
    >
      {/* Children - always rendered but blurred when loading */}
      <div
        className={`transition-all duration-500 ${
          isLoading
            ? "pointer-events-none scale-95 opacity-30 blur-md"
            : "scale-100 opacity-100 blur-none"
        }`}
      >
        {children}
      </div>

      {/* Loading overlay */}
      {isLoading && (
        <div className="fixed inset-0 z-[9999] flex items-center justify-center bg-black/20 backdrop-blur-sm duration-500 animate-in fade-in-0">
          <Loading />
        </div>
      )}
    </div>
  );
};

export default PageWrapper;
