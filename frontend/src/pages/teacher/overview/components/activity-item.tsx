import React from 'react';
import type { LucideIcon } from 'lucide-react';

interface ActivityItemProps {
  icon: LucideIcon;
  iconColor: string;
  iconBgColor: string;
  title: string;
  subtitle: string;
  time: string;
  badge?: {
    text: string;
    color: string;
  };
}

const ActivityItem = ({
  icon: Icon,
  iconColor,
  iconBgColor,
  title,
  subtitle,
  time,
  badge,
}: ActivityItemProps) => {
  //! State

  //! Functions

  //! Render
  return (
    <div className="flex items-center space-x-4 rounded-lg p-4 transition-colors hover:bg-gray-50">
      <div className={`rounded-lg p-2 ${iconBgColor}`}>
        <Icon className={`h-5 w-5 ${iconColor}`} />
      </div>
      <div className="min-w-0 flex-1">
        <p className="truncate text-sm font-medium text-gray-900">{title}</p>
        <p className="text-sm text-gray-500">{subtitle}</p>
      </div>
      <div className="flex items-center space-x-2">
        {badge && (
          <span className={`rounded-full px-2 py-1 text-xs font-medium ${badge.color}`}>
            {badge.text}
          </span>
        )}
        <span className="text-sm text-gray-500">{time}</span>
      </div>
    </div>
  );
};

export default React.memo(ActivityItem);
