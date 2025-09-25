import { Button } from '@/components/ui/button';
import { ScrollArea } from '@/components/ui/scroll-area';
import BaseUrl from '@/consts/baseUrl';
import useToggleDialog from '@/hooks/useToggleDialog';
import type { UserInfo } from '@/interfaces/user';
import { cn } from '@/lib/utils';
import httpService from '@/services/httpService';
import {
  Album,
  BookOpen,
  ClipboardList,
  Home,
  Keyboard,
  Landmark,
  LockIcon,
  LogOut,
  Menu,
  MessagesSquare,
  ShieldAlert,
  Tv,
  User,
  User2Icon,
  Users,
} from 'lucide-react';
import type React from 'react';
import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useLocation, useNavigate } from 'react-router-dom';
import NavigationBar from './components/NavigationBar';
import SideBar from './components/SideBar';
interface AdminLayoutProps {
  children: React.ReactNode;
}

const AdminLayout = (props: AdminLayoutProps) => {
  // State
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const { t } = useTranslation('shared');
  const [openAskLogout, toggleAskLogout, shouldRenderAskLogout] = useToggleDialog();
  const navigate = useNavigate();
  const location = useLocation();
  // Get current pathname
  const pathname = location.pathname;

  const toggleSidebar = () => {
    setSidebarOpen(!sidebarOpen);
  };

  const userData = httpService.getUserStorage() || {};
  const user: UserInfo = userData ? (userData as UserInfo) : ({} as UserInfo);

  // Menu items with enhanced structure
  const teacherMenuItems = [
    {
      title: t('SideBar.PermissionManagement'),
      href: BaseUrl.AdminPermission,
      icon: LockIcon,
    },
    {
      title: t('SideBar.UserManagement'),
      href: BaseUrl.AdminManageUsers,
      icon: User2Icon,
    },
    {
      title: t('SideBar.SubjectManagement'),
      href: BaseUrl.AdminManageSubject,
      icon: BookOpen,
    },
    {
      title: t('BankQuestion.ManageBankQuestion'),
      href: BaseUrl.AdminBankQuestion,
      icon: Landmark,
    },
    {
      title: t('SideBar.ClassManagement'),
      href: BaseUrl.AdminManageClass,
      icon: Users,
    },
    {
      title: t('SideBar.ExamRoomManagement'),
      href: BaseUrl.AdminManageRoom,
      icon: Home,
    },
    {
      title: t('SideBar.ProhibitedManagement'),
      href: BaseUrl.AdminProhibited,
      icon: ShieldAlert,
    },
    {
      title: t('SideBar.KeyboardShortcutManagement'),
      href: BaseUrl.AdminKeyboardShortcut,
      icon: Keyboard,
    },
    {
      title: t('SideBar.UserActivityLog'),
      href: BaseUrl.AdminUserActivityLog,
      icon: ClipboardList,
    },
    {
      title: t('SideBar.ExamSupervision'),
      href: BaseUrl.AdminSupervision,
      icon: Tv,
    },
    {
      title: t('SideBar.ExamManagement'),
      href: BaseUrl.AdminManageExam,
      icon: Album,
    },
    {
      title: t('SideBar.Feedback'),
      href: BaseUrl.ViewFeedback,
      icon: MessagesSquare,
    },
  ];

  const navBarItems = [
    {
      label: t('Navigation.Profile'),
      icon: <User className="mr-2 h-4 w-4" />,
      action: () => {
        navigate(`${BaseUrl.AdminProfile}`);
      },
    },
    {
      label: t('Logout'),
      icon: <LogOut className="mr-2 h-4 w-4" />,
      action: toggleAskLogout,
      className: 'text-red-600',
    },
  ];

  // Function to check if a menu item is active
  const isMenuItemActive = (href: string) => {
    // Exact match
    if (pathname === href) return true;

    // Check if current path starts with the menu item path (for nested routes)
    if (pathname.startsWith(href) && href !== '/') return true;

    return false;
  };

  // Add active state to menu items
  const enhancedTeacherMenuItems = teacherMenuItems.map((item) => ({
    ...item,
    isActive: isMenuItemActive(item.href),
  }));

  return (
    <main className="component:AdminLayout flex h-screen w-screen">
      {/* Sidebar */}
      <SideBar
        isOpen={sidebarOpen}
        onToggle={toggleSidebar}
        teacherMenuItems={enhancedTeacherMenuItems}
        user={user}
      />

      {/* Main Content */}
      <div
        className={cn(
          'flex flex-1 flex-col transition-all duration-300',
          sidebarOpen ? 'md:ml-0' : 'md:ml-0',
        )}
      >
        {/* Mobile Menu Button */}
        <div className="border-b border-border bg-background p-4 md:hidden">
          <Button variant="ghost" size="sm" onClick={toggleSidebar} className="h-8 w-8 p-0">
            <Menu className="h-4 w-4" />
          </Button>
        </div>

        <ScrollArea className="main-container bg-bgContainerContent h-full w-full">
          <NavigationBar
            navBarItems={navBarItems}
            openAskLogout={openAskLogout}
            toggleAskLogout={toggleAskLogout}
            user={user}
            shouldRenderAskLogout={shouldRenderAskLogout}
            teacherMenuItems={enhancedTeacherMenuItems}
          />
          <div className={cn('main-container__content min-h-[calc(100vh-121px)] bg-gray-100 p-4')}>
            {props.children}
          </div>
        </ScrollArea>
      </div>
    </main>
  );
};

export default AdminLayout;
