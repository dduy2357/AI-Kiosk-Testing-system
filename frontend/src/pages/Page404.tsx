import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { ArrowLeft, Search, Sparkles } from "lucide-react";
import React, { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";

const Page404 = () => {
  //! State
  const [searchQuery, setSearchQuery] = useState("");
  const [isVisible, setIsVisible] = useState(false);
  const navigate = useNavigate();

  //! Function
  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    // Implement search functionality here
  };

  useEffect(() => {
    setIsVisible(true);
  }, []);

  //! Render
  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-purple-50 via-blue-50 to-indigo-100 p-4 dark:from-gray-900 dark:via-purple-900 dark:to-indigo-900">
      <div
        className={`mx-auto max-w-2xl text-center transition-all duration-1000 ${
          isVisible ? "translate-y-0 opacity-100" : "translate-y-8 opacity-0"
        }`}
      >
        {/* Animated 404 Text */}
        <div className="relative mb-8">
          <div className="absolute inset-0 flex items-center justify-center">
            <div className="h-64 w-64 animate-pulse rounded-full bg-gradient-to-r from-purple-400 to-pink-400 opacity-20 blur-3xl"></div>
          </div>
          <h1 className="relative animate-bounce bg-gradient-to-r from-purple-600 via-pink-600 to-blue-600 bg-clip-text text-9xl font-black text-transparent md:text-[12rem]">
            404
          </h1>
          <div className="animate-spin-slow absolute right-4 top-4">
            <Sparkles className="h-8 w-8 text-yellow-400" />
          </div>
          <div className="absolute bottom-4 left-4 animate-bounce delay-300">
            <Sparkles className="h-6 w-6 text-pink-400" />
          </div>
        </div>

        {/* Error Message */}
        <div className="mb-8 space-y-4">
          <h2 className="text-3xl font-bold text-gray-800 dark:text-white md:text-4xl">
            Oops! Page Not Found
          </h2>
          <p className="mx-auto max-w-md text-lg text-gray-600 dark:text-gray-300">
            The page you're looking for seems to have wandered off into the
            digital void. Don't worry, even the best explorers get lost
            sometimes!
          </p>
        </div>

        {/* Search Bar */}
        <form onSubmit={handleSearch} className="mx-auto mb-8 max-w-md">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 transform text-gray-400" />
            <Input
              type="text"
              placeholder="Search for what you need..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full rounded-full border-2 border-purple-200 py-3 pl-10 pr-4 transition-colors focus:border-purple-400"
            />
          </div>
        </form>

        {/* Action Buttons */}
        <div className="mb-8 flex flex-col items-center justify-center gap-4 sm:flex-row">
          <Button
            variant="outline"
            onClick={() => navigate(-1)}
            className="transform rounded-full bg-gradient-to-r from-purple-600 to-pink-600 px-8 py-3 text-white shadow-lg transition-all duration-300 hover:scale-105 hover:from-purple-700 hover:to-pink-700"
          >
            <ArrowLeft className="mr-2 h-5 w-5" />
            Go Back
          </Button>
        </div>

        {/* Helpful Links */}
        <div className="grid grid-cols-2 gap-4 text-sm md:grid-cols-4">
          <Link
            to="/about"
            className="text-purple-600 transition-colors hover:text-purple-800 hover:underline"
          >
            About Us
          </Link>
          <Link
            to="/contact"
            className="text-purple-600 transition-colors hover:text-purple-800 hover:underline"
          >
            Contact
          </Link>
          <Link
            to="/help"
            className="text-purple-600 transition-colors hover:text-purple-800 hover:underline"
          >
            Help Center
          </Link>
          <Link
            to="/sitemap"
            className="text-purple-600 transition-colors hover:text-purple-800 hover:underline"
          >
            Sitemap
          </Link>
        </div>

        {/* Floating Elements */}
        <div className="absolute left-10 top-20 h-4 w-4 animate-float rounded-full bg-purple-400 opacity-60"></div>
        <div className="absolute right-20 top-40 h-6 w-6 animate-float rounded-full bg-pink-400 opacity-60 delay-1000"></div>
        <div className="absolute bottom-20 left-20 h-3 w-3 animate-float rounded-full bg-blue-400 opacity-60 delay-500"></div>
        <div className="delay-1500 absolute bottom-40 right-10 h-5 w-5 animate-float rounded-full bg-indigo-400 opacity-60"></div>
      </div>

      <style>{`
        @keyframes float {
          0%,
          100% {
            transform: translateY(0px);
          }
          50% {
            transform: translateY(-20px);
          }
        }
        @keyframes spin-slow {
          from {
            transform: rotate(0deg);
          }
          to {
            transform: rotate(360deg);
          }
        }
        .animate-float {
          animation: float 3s ease-in-out infinite;
        }
        .animate-spin-slow {
          animation: spin-slow 8s linear infinite;
        }
      `}</style>
    </div>
  );
};

export default Page404;
