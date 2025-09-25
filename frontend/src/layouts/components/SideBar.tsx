import { ImageSource } from '@/assets';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Button } from '@/components/ui/button';
import { Separator } from '@/components/ui/separator';
import type { UserInfo } from '@/interfaces/user';
import { cn } from '@/lib/utils';
import httpService from '@/services/httpService';
import { MenuIcon, User } from 'lucide-react';
import React from 'react';
import { useTranslation } from 'react-i18next';
import MenuItem from './MenuItem';

interface SubItem {
  title: string;
  href: string;
  icon?: React.ComponentType<React.SVGProps<SVGSVGElement>>;
  isActive?: boolean;
}

interface MenuItemType {
  title: string;
  subtitle?: string;
  href: string;
  icon: React.ComponentType<React.SVGProps<SVGSVGElement>>;
  isAction?: boolean;
  isActive?: boolean;
  subItems?: SubItem[];
}

interface SideBarProps {
  isOpen: boolean;
  onToggle: () => void;
  user?: UserInfo | null;
  studentMenuItems?: MenuItemType[];
  teacherMenuItems?: MenuItemType[];
}

const SideBar = ({ isOpen, onToggle, user, studentMenuItems, teacherMenuItems }: SideBarProps) => {
  const { t } = useTranslation('shared');
  const roleId = Number(httpService.getUserStorage()?.roleId) || '';

  return (
    <>
      {/* Mobile Overlay with enhanced backdrop */}
      {isOpen && (
        <button
          type="button"
          className="fixed inset-0 z-40 bg-black/60 backdrop-blur-md transition-all duration-300 md:hidden"
          onClick={onToggle}
          aria-label="Close overlay"
        />
      )}

      {/* Enhanced Sidebar */}
      <aside
        className={cn(
          'fixed left-0 top-0 z-50 h-full transition-all duration-500 ease-out',
          'flex flex-col shadow-2xl',
          'bg-gradient-to-b from-slate-50 via-white to-slate-50/80 dark:from-slate-900 dark:via-slate-800 dark:to-slate-900/80',
          'border-r border-slate-200/60 dark:border-slate-700/60',
          'backdrop-blur-xl',
          isOpen ? 'w-72' : 'w-16',
          'md:relative md:translate-x-0',
          !isOpen && '-translate-x-full md:translate-x-0',
        )}
      >
        {/* Enhanced Header */}
        <div className="relative border-b border-slate-200/60 bg-gradient-to-r from-blue-50/50 to-purple-50/50 dark:border-slate-700/60 dark:from-blue-950/30 dark:to-purple-950/30">
          <div className="flex items-center justify-between p-4">
            {isOpen && (
              <div className="flex items-center gap-3">
                <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-gradient-to-br from-blue-500 to-purple-600 shadow-lg">
                  <img src={ImageSource.logoApp} alt="Logo" className="h-8 w-8 rounded-lg" />
                </div>
                <div className="flex flex-col">
                  <span className="text-md whitespace-nowrap bg-gradient-to-r from-blue-600 via-purple-600 to-blue-800 bg-clip-text font-bold text-transparent">
                    {t('AKTs')}
                  </span>
                  <span className="text-xs text-slate-500 dark:text-slate-400">
                    {roleId === 4 ? 'Admin' : 'Teacher'} Panel
                  </span>
                </div>
              </div>
            )}
            <Button
              variant="ghost"
              size="sm"
              onClick={onToggle}
              className={cn(
                'h-9 w-9 rounded-lg transition-all duration-200 hover:bg-slate-100 dark:hover:bg-slate-800',
                'hover:scale-105 active:scale-95',
                !isOpen && 'mx-auto bg-white/80 shadow-md dark:bg-slate-800/80',
              )}
            >
              <MenuIcon className="h-4 w-4 text-slate-600 dark:text-slate-300" />
            </Button>
          </div>
          {/* Decorative gradient line */}
          <div className="absolute bottom-0 left-0 right-0 h-px bg-gradient-to-r from-transparent via-blue-500/30 to-transparent" />
        </div>

        {/* Enhanced Navigation */}
        <nav className="scrollbar-thin scrollbar-track-transparent scrollbar-thumb-slate-300 dark:scrollbar-thumb-slate-600 flex-1 space-y-6 overflow-y-auto p-4">
          {/* Admin Management Section */}
          <div className="space-y-3">
            {isOpen && (
              <div className="flex items-center gap-2 px-2">
                <div className="flex h-6 w-6 items-center justify-center rounded-md bg-gradient-to-br from-blue-100 to-purple-100 dark:from-blue-900/50 dark:to-purple-900/50">
                  <User className="h-3 w-3 text-blue-600 dark:text-blue-400" />
                </div>
                <h3 className="text-sm font-semibold text-slate-700 dark:text-slate-300">
                  {t('SideBar.SystemManagement')}
                </h3>
              </div>
            )}
            <div className="space-y-1">
              {(studentMenuItems || teacherMenuItems)?.map((item, index) => (
                <MenuItem key={index} item={item} isCollapsed={!isOpen} isActive={item.isActive} />
              ))}
            </div>
          </div>

          {/* Enhanced Separator */}
          <div className="relative">
            <Separator className="bg-gradient-to-r from-transparent via-slate-300 to-transparent dark:via-slate-600" />
            <div className="absolute left-1/2 top-1/2 h-2 w-2 -translate-x-1/2 -translate-y-1/2 rounded-full bg-slate-300 dark:bg-slate-600" />
          </div>
        </nav>

        {/* Enhanced Footer */}
        {isOpen && (
          <div className="border-t border-slate-200/60 bg-gradient-to-r from-slate-50/50 to-slate-100/50 p-4 dark:border-slate-700/60 dark:from-slate-800/50 dark:to-slate-900/50">
            <div className="group flex items-center gap-3 rounded-xl bg-white/60 p-3 shadow-sm transition-all duration-200 hover:bg-white/80 hover:shadow-md dark:bg-slate-800/60 dark:hover:bg-slate-800/80">
              <div className="relative">
                <Avatar className="h-10 w-10 shadow-lg ring-2 ring-white dark:ring-slate-700">
                  <AvatarImage
                    src={user?.avatarUrl ?? ''}
                    alt={user?.fullName}
                    className="object-cover"
                  />
                  <AvatarFallback className="bg-gradient-to-br from-blue-500 via-purple-500 to-pink-500 font-semibold text-white">
                    {user?.fullName
                      ?.split(' ')
                      .map((n) => n[0])
                      .join('')
                      .slice(0, 2)}
                  </AvatarFallback>
                </Avatar>
                <div className="absolute -bottom-0.5 -right-0.5 h-3 w-3 rounded-full bg-emerald-500 ring-2 ring-white dark:ring-slate-800" />
              </div>
              <div className="min-w-0 flex-1">
                <p className="truncate font-semibold text-slate-800 transition-colors group-hover:text-blue-600 dark:text-slate-200 dark:group-hover:text-blue-400">
                  {user?.fullName}
                </p>
                <p className="truncate text-xs text-slate-500 dark:text-slate-400">{user?.email}</p>
              </div>
              <div className="opacity-0 transition-opacity group-hover:opacity-100">
                <div className="h-2 w-2 rounded-full bg-blue-500" />
              </div>
            </div>
          </div>
        )}

        {/* Decorative elements */}
        <div className="absolute -right-px top-20 h-32 w-px bg-gradient-to-b from-transparent via-blue-500/20 to-transparent" />
        <div className="absolute -right-px bottom-32 h-24 w-px bg-gradient-to-b from-transparent via-purple-500/20 to-transparent" />
      </aside>
    </>
  );
};

export default React.memo(SideBar);
