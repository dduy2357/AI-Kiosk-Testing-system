import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import httpService from '@/services/httpService';
import { AlertTriangle, ArrowLeft, Eye, EyeOff, Home, Key, Lock, Mail, Shield } from 'lucide-react';
import React, { useEffect, useState } from 'react';

interface AccessDeniedProps {
  title?: string;
  message?: string;
  showContactButton?: boolean;
  adminEmail?: string;
  onBack?: () => void;
}

const AccessDenied = ({
  title = 'Access Denied',
  message = "You don't have permission to access this resource. Please contact your administrator or sign in with appropriate credentials.",
  showContactButton = true,
  adminEmail = 'admin@example.com',
  onBack,
}: AccessDeniedProps) => {
  //! State
  const [isVisible, setIsVisible] = useState(false);
  const [isLocked, setIsLocked] = useState(true);

  //! Function

  const handleBack = () => {
    if (onBack) {
      onBack();
    } else {
      window.history.back();
    }
  };

  const handleContactAdmin = () => {
    window.location.href = `mailto:${adminEmail}?subject=Access Request&body=I need access to this resource.`;
  };

  const toggleLock = () => {
    setIsLocked(!isLocked);
  };

  useEffect(() => {
    setIsVisible(true);
  }, []);

  //! Render
  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-red-50 via-orange-50 to-yellow-50 p-4 dark:from-gray-900 dark:via-red-900 dark:to-orange-900">
      <div
        className={`mx-auto max-w-2xl text-center transition-all duration-1000 ${
          isVisible ? 'translate-y-0 opacity-100' : 'translate-y-8 opacity-0'
        }`}
      >
        {/* Animated Security Icon */}
        <div className="relative mb-8">
          <div className="absolute inset-0 flex items-center justify-center">
            <div className="h-48 w-48 animate-pulse rounded-full bg-gradient-to-r from-red-400 to-orange-400 opacity-20 blur-3xl"></div>
          </div>

          <div className="relative mb-4 flex items-center justify-center">
            <button
              type="button"
              onClick={toggleLock}
              aria-pressed={isLocked}
              aria-label={isLocked ? 'Unlock' : 'Lock'}
              className="relative cursor-pointer transition-transform duration-300 hover:scale-110"
            >
              <Shield className="h-32 w-32 animate-pulse text-red-500" />
              <div className="absolute inset-0 flex items-center justify-center">
                {isLocked ? (
                  <Lock className="h-16 w-16 animate-bounce text-red-600" />
                ) : (
                  <Key className="h-16 w-16 animate-spin text-orange-600" />
                )}
              </div>
            </button>
          </div>

          {/* Floating Warning Icons */}
          <div className="absolute left-0 top-0 animate-float">
            <AlertTriangle className="h-6 w-6 text-yellow-500" />
          </div>
          <div className="absolute right-0 top-4 animate-float delay-1000">
            <Eye className="h-5 w-5 text-red-400" />
          </div>
          <div className="absolute bottom-0 right-4 animate-float delay-500">
            <EyeOff className="h-4 w-4 text-orange-400" />
          </div>
        </div>

        {/* Error Message Card */}
        <Card className="mb-8 border-2 border-red-200 bg-white/80 shadow-2xl backdrop-blur-sm dark:border-red-800 dark:bg-gray-800/80">
          <CardContent className="p-8">
            <h1 className="mb-4 text-4xl font-bold text-red-600 dark:text-red-400 md:text-5xl">
              {title}
            </h1>
            <div className="mb-4 flex items-center justify-center">
              <div className="h-1 w-20 rounded-full bg-gradient-to-r from-red-500 to-orange-500"></div>
            </div>
            <p className="mx-auto max-w-lg text-lg leading-relaxed text-gray-700 dark:text-gray-300">
              {message}
            </p>
          </CardContent>
        </Card>

        {/* Action Buttons */}
        <div className="mb-8 flex flex-col items-center justify-center gap-4 sm:flex-row">
          <Button
            variant="outline"
            onClick={handleBack}
            className="transform rounded-full border-2 border-red-300 px-8 py-3 text-red-700 transition-all duration-300 hover:scale-105 hover:bg-red-50 dark:border-red-700 dark:text-red-400 dark:hover:bg-red-900/20"
          >
            <ArrowLeft className="mr-2 h-5 w-5" />
            Go Back
          </Button>

          <Button
            onClick={() => {
              httpService.clearStorage();
              window.location.reload();
            }}
            variant="ghost"
            className="transform rounded-full px-8 py-3 text-gray-600 transition-all duration-300 hover:scale-105 hover:text-gray-800 dark:text-gray-400 dark:hover:text-gray-200"
          >
            <Home className="mr-2 h-5 w-5" />
            Home
          </Button>
        </div>

        {/* Contact Section */}
        {showContactButton && (
          <div className="rounded-2xl border border-red-200 bg-white/60 p-6 backdrop-blur-sm dark:border-red-800 dark:bg-gray-800/60">
            <h3 className="mb-3 text-lg font-semibold text-gray-800 dark:text-gray-200">
              Need Access?
            </h3>
            <p className="mb-4 text-sm text-gray-600 dark:text-gray-400">
              Contact your administrator to request access to this resource.
            </p>
            <Button
              onClick={handleContactAdmin}
              variant="outline"
              size="sm"
              className="border-orange-300 text-orange-700 hover:bg-orange-50 dark:border-orange-700 dark:text-orange-400 dark:hover:bg-orange-900/20"
            >
              <Mail className="mr-2 h-4 w-4" />
              Contact Admin
            </Button>
          </div>
        )}

        {/* Security Notice */}
        <div className="mt-8 flex items-center justify-center gap-2 text-xs text-gray-500 dark:text-gray-400">
          <Shield className="h-4 w-4" />
          <span>This area is protected for security reasons</span>
        </div>

        {/* Floating Security Elements */}
        <div className="absolute left-10 top-20 h-3 w-3 animate-float rounded-full bg-red-400 opacity-60"></div>
        <div className="absolute right-20 top-40 h-4 w-4 animate-float rounded-full bg-orange-400 opacity-60 delay-1000"></div>
        <div className="absolute bottom-20 left-20 h-2 w-2 animate-float rounded-full bg-yellow-400 opacity-60 delay-500"></div>
        <div className="delay-1500 absolute bottom-40 right-10 h-5 w-5 animate-float rounded-full bg-red-300 opacity-60"></div>
      </div>

      <style>{`
        @keyframes float {
          0%,
          100% {
            transform: translateY(0px);
          }
          50% {
            transform: translateY(-15px);
          }
        }
        .animate-float {
          animation: float 4s ease-in-out infinite;
        }
      `}</style>
    </div>
  );
};

export default React.memo(AccessDenied);
