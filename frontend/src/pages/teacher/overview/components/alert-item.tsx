import { Button } from '@/components/ui/button';
import { AlertTriangle } from 'lucide-react';
import React from 'react';

interface AlertItemProps {
  title: string;
  location: string;
  time: string;
  severity: 'high' | 'medium' | 'low';
  type: 'error' | 'warning' | 'info';
}

const AlertItem = ({ title, location, time, severity }: AlertItemProps) => {
  //! State
  const getSeverityConfig = () => {
    switch (severity) {
      case 'high':
        return {
          badge: 'bg-red-100 text-red-800',
          text: 'Cao',
          icon: 'text-red-600',
        };
      case 'medium':
        return {
          badge: 'bg-yellow-100 text-yellow-800',
          text: 'Trung bình',
          icon: 'text-yellow-600',
        };
      case 'low':
        return {
          badge: 'bg-blue-100 text-blue-800',
          text: 'Thấp',
          icon: 'text-blue-600',
        };
      default:
        return {
          badge: 'bg-gray-100 text-gray-800',
          text: 'Thấp',
          icon: 'text-gray-600',
        };
    }
  };

  //! Functions

  //! Render
  const config = getSeverityConfig();

  return (
    <div className="flex items-center justify-between rounded-lg p-4 transition-colors hover:bg-gray-50">
      <div className="flex items-center space-x-3">
        <AlertTriangle className={`h-5 w-5 ${config.icon}`} />
        <div>
          <p className="text-sm font-medium text-gray-900">{title}</p>
          <p className="text-sm text-gray-500">
            {location} • {time}
          </p>
        </div>
      </div>
      <div className="flex items-center space-x-2">
        <span className={`rounded-full px-2 py-1 text-xs font-medium ${config.badge}`}>
          {config.text}
        </span>
        <Button variant="ghost" size="sm" className="text-blue-600 hover:text-blue-700">
          Chi tiết
        </Button>
      </div>
    </div>
  );
};

export default React.memo(AlertItem);
