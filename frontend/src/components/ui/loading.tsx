import clsx from "clsx";
import CommonIcons from "../commonIcons";

const Loading = ({ className }: { className?: string }) => {
  return (
    <div className="flex flex-col items-center justify-center space-y-6">
      {/* Main loading container with glass effect */}
      <div className="relative p-8">
        {/* Animated background gradient */}
        <div className=""></div>

        {/* Loading spinner with multiple layers */}
        <div className="relative flex flex-col items-center space-y-4">
          <div className="relative">
            {/* Outer rotating ring */}
            <div className="absolute inset-0 h-16 w-16 animate-spin rounded-full border-4 border-transparent border-r-blue-500 border-t-blue-500"></div>

            {/* Middle ring with different speed */}
            <div className="absolute inset-1 h-14 w-14 animate-spin rounded-full border-4 border-transparent border-l-purple-500 border-t-purple-500 [animation-direction:reverse] [animation-duration:1.5s]"></div>

            {/* Inner pulsing circle */}
            <div className="absolute inset-3 h-10 w-10 animate-pulse rounded-full bg-gradient-to-r from-blue-500 to-purple-500"></div>

            {/* Center icon */}
            <div className="relative flex h-16 w-16 items-center justify-center">
              <CommonIcons.Loader2
                className={clsx("h-6 w-6 animate-spin text-white", className)}
              />
            </div>
          </div>

          {/* Loading text with typewriter effect */}
          <div className="space-y-2 text-center">
            <div className="flex items-center justify-center space-x-1">
              <span className="text-lg font-semibold text-white">Loading</span>
              <div className="flex space-x-1">
                <div className="h-1.5 w-1.5 animate-bounce rounded-full bg-blue-500 [animation-delay:-0.3s]"></div>
                <div className="h-1.5 w-1.5 animate-bounce rounded-full bg-purple-500 [animation-delay:-0.15s]"></div>
                <div className="h-1.5 w-1.5 animate-bounce rounded-full bg-pink-500"></div>
              </div>
            </div>
            <p className="text-sm text-gray-600">
              Please wait while we prepare your content
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Loading;
