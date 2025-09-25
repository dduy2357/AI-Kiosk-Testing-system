import { useMemo } from 'react';
import { useLocation } from 'react-router-dom';

interface MenuItemType {
  title: string;
  href: string;
  subItems?: Array<{
    title: string;
    href: string;
  }>;
}

export const useActivePath = () => {
  const location = useLocation();
  const pathname = location.pathname;

  const isMenuItemActive = useMemo(() => {
    return (href: string) => {
      // Exact match
      if (pathname === href) return true;

      // Check if current path starts with the menu item path (for nested routes)
      if (pathname.startsWith(href) && href !== '/') return true;

      return false;
    };
  }, [pathname]);

  const getActiveMenuItem = useMemo(() => {
    return (menuItems: MenuItemType[]) => {
      return menuItems.find((item) => {
        // Check main item
        if (isMenuItemActive(item.href)) return true;

        // Check sub items
        if (item.subItems) {
          return item.subItems.some((subItem) => isMenuItemActive(subItem.href));
        }

        return false;
      });
    };
  }, [isMenuItemActive]);

  return {
    pathname,
    isMenuItemActive,
    getActiveMenuItem,
  };
};
