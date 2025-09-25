import { Button } from '@/components/ui/button';
import { ScrollArea } from '@/components/ui/scroll-area';
import BaseUrl from '@/consts/baseUrl';
import useToggleDialog from '@/hooks/useToggleDialog';
import { UserInfo } from '@/interfaces/user';
import { cn } from '@/lib/utils';
import httpService from '@/services/httpService';
import {
  Album,
  FileQuestion,
  Home,
  HomeIcon,
  LogOut,
  Menu,
  MessagesSquare,
  Newspaper,
  Tv,
  User,
} from 'lucide-react';
import type React from 'react';
import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useLocation, useNavigate } from 'react-router-dom';
import NavigationBar from './components/NavigationBar';
import SideBar from './components/SideBar';

interface TeacherLayoutProps {
  children: React.ReactNode;
}

const TeacherLayout = (props: TeacherLayoutProps) => {
  //!State
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const { t } = useTranslation('shared');
  const [openAskLogout, toggleAskLogout, shouldRenderAskLogout] = useToggleDialog();
  const location = useLocation();
  const navigate = useNavigate();
  const pathname = location.pathname;

  const toggleSidebar = () => {
    setSidebarOpen(!sidebarOpen);
  };

  const userData = httpService.getUserStorage() || {};
  const user: UserInfo = userData ? (userData as UserInfo) : ({} as UserInfo);

  const teacherMenuItems = [
    {
      title: t('SideBar.Overview'),
      subtitle: t('SideBar.ManinDashboard'),
      href: BaseUrl.Overview,
      icon: HomeIcon,
    },
    {
      title: t('SideBar.ManageQuestion'),
      href: BaseUrl.BankQuestion,
      icon: FileQuestion,
    },
    {
      title: t('SideBar.ExamManagement'),
      href: BaseUrl.ManageExam,
      icon: Album,
    },
    {
      title: t('SideBar.ExamRoomManagement'),
      href: BaseUrl.TeacherManageRoom,
      icon: Home,
    },
    {
      title: t('SideBar.ExamResultManagement'),
      href: BaseUrl.ExamResultTeacher,
      icon: Newspaper,
    },
    {
      title: t('SideBar.ExamSupervision'),
      href: BaseUrl.ExamSupervision,
      icon: Tv,
    },
    {
      title: t('SideBar.Feedback'),
      href: BaseUrl.SendFeedbackTeacher,
      icon: MessagesSquare,
    },
  ];

  const navBarItems = [
    {
      label: t('Navigation.Profile'),
      icon: <User className="mr-2 h-4 w-4" />,
      action: () => {
        navigate(`${BaseUrl.TeacherProfile}`);
      },
    },
    {
      label: t('Logout'),
      icon: <LogOut className="mr-2 h-4 w-4" />,
      action: toggleAskLogout,
      className: 'text-red-600',
    },
  ];

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

  //!Functions

  //!Render
  return (
    <main className="component:TeacherLayout flex h-screen w-screen">
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
            teacherMenuItems={teacherMenuItems}
          />
          <div className={cn('main-container__content min-h-[calc(100vh-121px) p-4')}>
            {props.children}
          </div>
        </ScrollArea>
      </div>
    </main>
  );
};

export default TeacherLayout;
