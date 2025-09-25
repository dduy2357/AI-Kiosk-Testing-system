import { Button } from '@/components/ui/button';
import { ScrollArea } from '@/components/ui/scroll-area';
import BaseUrl from '@/consts/baseUrl';
import useToggleDialog from '@/hooks/useToggleDialog';
import { UserInfo } from '@/interfaces/user';
import { cn } from '@/lib/utils';
import httpService from '@/services/httpService';
import { BarChart3, FileText, LogOut, Menu, MessagesSquare, User } from 'lucide-react';
import type React from 'react';
import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import NavigationBar from './components/NavigationBar';
import SideBar from './components/SideBar';

interface DefaultLayoutProps {
  children: React.ReactNode;
}

const DefaultLayout = (props: DefaultLayoutProps) => {
  //!State
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const { t } = useTranslation('shared');
  const [openAskLogout, toggleAskLogout, shouldRenderAskLogout] = useToggleDialog();
  const navigate = useNavigate();

  const toggleSidebar = () => {
    setSidebarOpen(!sidebarOpen);
  };
  const userData = httpService.getUserStorage() || {};
  const user: UserInfo = userData ? (userData as UserInfo) : ({} as UserInfo);
  const studentMenuItems = [
    {
      title: t('SideBar.ExamList'),
      href: BaseUrl.ExamList,
      icon: FileText,
    },
    {
      title: t('SideBar.ExamListResult'),
      href: BaseUrl.ExamResult,
      icon: BarChart3,
    },
    {
      title: t('SideBar.Feedback'),
      href: BaseUrl.SendFeedback,
      icon: MessagesSquare,
    },
  ];

  const navBarItems = [
    {
      label: t('Navigation.Profile'),
      icon: <User className="mr-2 h-4 w-4" />,
      action: () => {
        navigate(`${BaseUrl.StudentProfile}`);
      },
    },
    {
      label: t('Logout'),
      icon: <LogOut className="mr-2 h-4 w-4" />,
      action: toggleAskLogout,
      className: 'text-red-600',
    },
  ];

  //!Functions

  //!Render
  return (
    <main className="component:DefaultLayout flex h-screen w-screen">
      {/* Sidebar */}
      <SideBar
        isOpen={sidebarOpen}
        onToggle={toggleSidebar}
        studentMenuItems={studentMenuItems}
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
          />
          <div className={cn('main-container__content min-h-[90vh] bg-gray-100')}>
            {props.children}
          </div>
          {/* <Footer /> */}
        </ScrollArea>
      </div>
    </main>
  );
};

export default DefaultLayout;
