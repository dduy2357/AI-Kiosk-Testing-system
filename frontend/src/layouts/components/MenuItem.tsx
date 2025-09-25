import type React from 'react';
import { useState } from 'react';
import { cn } from '@/lib/utils';
import { ChevronRight } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '@/components/ui/collapsible';
import { Link } from 'react-router-dom';

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

interface MenuItemProps {
  item: MenuItemType;
  isCollapsed: boolean;
  isActive?: boolean;
}

const MenuItem = ({ item, isCollapsed, isActive = false }: MenuItemProps) => {
  const [isOpen, setIsOpen] = useState(false);
  const hasSubItems = item.subItems && item.subItems.length > 0;

  const IconComponent = item.icon;

  // Use the isActive prop passed from parent or fallback to item.isActive
  const activeState = isActive || item.isActive || false;

  if (hasSubItems) {
    return (
      <Collapsible open={isOpen} onOpenChange={setIsOpen}>
        <CollapsibleTrigger asChild>
          <Button
            variant="ghost"
            className={cn(
              'group relative w-full justify-start gap-3 rounded-xl px-3 py-2.5 text-left transition-all duration-200',
              'hover:bg-gradient-to-r hover:from-blue-50 hover:to-purple-50 dark:hover:from-blue-950/50 dark:hover:to-purple-950/50',
              'hover:scale-[1.02] hover:shadow-sm active:scale-[0.98]',
              activeState &&
                'border border-blue-200/50 bg-gradient-to-r from-blue-100 to-purple-100 shadow-sm dark:border-blue-800/50 dark:from-blue-900/50 dark:to-purple-900/50',
              isCollapsed && 'justify-center px-2',
            )}
          >
            <div className="flex flex-1 items-center gap-3">
              <div
                className={cn(
                  'flex items-center justify-center rounded-lg transition-all duration-200',
                  'group-hover:rotate-3 group-hover:scale-110',
                  activeState && 'bg-blue-100 p-1 dark:bg-blue-900/50',
                  isCollapsed ? 'h-6 w-6' : 'h-5 w-5',
                )}
              >
                <IconComponent
                  className={cn(
                    'transition-colors duration-200',
                    activeState
                      ? 'text-blue-600 dark:text-blue-400'
                      : 'text-slate-600 group-hover:text-blue-600 dark:text-slate-400 dark:group-hover:text-blue-400',
                    isCollapsed ? 'h-5 w-5' : 'h-4 w-4',
                  )}
                />
              </div>
              {!isCollapsed && (
                <>
                  <div className="min-w-0 flex-1">
                    <span
                      className={cn(
                        'block truncate font-medium transition-colors duration-200',
                        activeState
                          ? 'text-blue-700 dark:text-blue-300'
                          : 'text-slate-700 group-hover:text-blue-700 dark:text-slate-300 dark:group-hover:text-blue-300',
                      )}
                    >
                      {item.title}
                    </span>
                    {item.subtitle && (
                      <span className="block truncate text-xs text-slate-500 dark:text-slate-400">
                        {item.subtitle}
                      </span>
                    )}
                  </div>
                  <ChevronRight
                    className={cn(
                      'ml-auto h-4 w-4 transition-all duration-200',
                      'text-slate-400 group-hover:text-blue-500',
                      isOpen && 'rotate-90',
                    )}
                  />
                </>
              )}
            </div>
            {/* Active indicator */}
            {activeState && (
              <div className="absolute left-0 top-1/2 h-8 w-1 -translate-y-1/2 rounded-r-full bg-gradient-to-b from-blue-500 to-purple-500" />
            )}
          </Button>
        </CollapsibleTrigger>
        {!isCollapsed && (
          <CollapsibleContent className="space-y-1 pl-6 pt-1">
            {item.subItems?.map((subItem, index) => (
              <Button
                key={index}
                variant="ghost"
                asChild
                className={cn(
                  'group relative w-full justify-start gap-3 rounded-lg px-3 py-2 text-left transition-all duration-200',
                  'hover:translate-x-1 hover:bg-slate-100 dark:hover:bg-slate-800/50',
                  subItem.isActive &&
                    'bg-blue-50 text-blue-700 dark:bg-blue-950/30 dark:text-blue-300',
                )}
              >
                <Link to={subItem.href}>
                  {subItem.icon && (
                    <subItem.icon className="h-3 w-3 text-slate-500 group-hover:text-blue-500 dark:text-slate-400" />
                  )}
                  <span className="text-sm text-slate-600 group-hover:text-slate-800 dark:text-slate-400 dark:group-hover:text-slate-200">
                    {subItem.title}
                  </span>
                  {subItem.isActive && (
                    <div className="absolute left-0 top-1/2 h-4 w-0.5 -translate-y-1/2 rounded-r-full bg-blue-400" />
                  )}
                </Link>
              </Button>
            ))}
          </CollapsibleContent>
        )}
      </Collapsible>
    );
  }

  return (
    <Button
      variant="ghost"
      asChild
      className={cn(
        'group relative w-full justify-start gap-3 rounded-xl px-3 py-2.5 text-left transition-all duration-200',
        'hover:bg-gradient-to-r hover:from-blue-50 hover:to-purple-50 dark:hover:from-blue-950/50 dark:hover:to-purple-950/50',
        'hover:scale-[1.02] hover:shadow-sm active:scale-[0.98]',
        activeState &&
          'border border-blue-200/50 bg-gradient-to-r from-blue-100 to-purple-100 shadow-sm dark:border-blue-800/50 dark:from-blue-900/50 dark:to-purple-900/50',
        isCollapsed && 'justify-center px-2',
      )}
    >
      <Link to={item.href}>
        <div className="flex items-center gap-3">
          <div
            className={cn(
              'flex items-center justify-center rounded-lg transition-all duration-200',
              'group-hover:rotate-3 group-hover:scale-110',
              activeState && 'bg-blue-100 p-1 dark:bg-blue-900/50',
              isCollapsed ? 'h-6 w-6' : 'h-5 w-5',
            )}
          >
            <IconComponent
              className={cn(
                'transition-colors duration-200',
                activeState
                  ? 'text-blue-600 dark:text-blue-400'
                  : 'text-slate-600 group-hover:text-blue-600 dark:text-slate-400 dark:group-hover:text-blue-400',
                isCollapsed ? 'h-5 w-5' : 'h-4 w-4',
              )}
            />
          </div>
          {!isCollapsed && (
            <div className="min-w-0 flex-1">
              <span
                className={cn(
                  'block truncate font-medium transition-colors duration-200',
                  activeState
                    ? 'text-blue-700 dark:text-blue-300'
                    : 'text-slate-700 group-hover:text-blue-700 dark:text-slate-300 dark:group-hover:text-blue-300',
                )}
              >
                {item.title}
              </span>
              {item.subtitle && (
                <span className="block truncate text-xs text-slate-500 dark:text-slate-400">
                  {item.subtitle}
                </span>
              )}
            </div>
          )}
        </div>
        {/* Active indicator */}
        {activeState && (
          <div className="absolute left-0 top-1/2 h-8 w-1 -translate-y-1/2 rounded-r-full bg-gradient-to-b from-blue-500 to-purple-500" />
        )}
      </Link>
    </Button>
  );
};

export default MenuItem;
