import { ImageSource } from '@/assets';
import DialogConfirm from '@/components/dialogs/DialogConfirm';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { NOTIFY_HUB } from '@/consts/apiUrl';
import cachedKeys from '@/consts/cachedKeys';
import { ROLE_ENUM } from '@/consts/role';
import { showError, showSuccess } from '@/helpers/toast';
import useFiltersHandler from '@/hooks/useFiltersHandler';
import useToggleDialog from '@/hooks/useToggleDialog';
import { KEY_LANG } from '@/i18n/config';
import type { UserInfo } from '@/interfaces/user';
import { useAuth } from '@/providers/AuthenticationProvider';
import alertService from '@/services/modules/alert/alert.service';
import useGetListAlert from '@/services/modules/alert/hooks/useGetListAlert';
import type { IAlertRequest } from '@/services/modules/alert/interfaces/alert.interface';
import createSignalRService from '@/services/signalRService';
import { useGet, useSave } from '@/stores/useStores';
import type * as signalR from '@microsoft/signalr';
import { Bell, ChevronDown, Clock, Mail, MailOpen, Trash2 } from 'lucide-react';
import React, { useEffect, useMemo, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link, useLocation } from 'react-router-dom';
import DialogDetailNotify from '../dialogs/DialogDetailNotify';
import { formatTimeAgo } from '@/helpers/common';

export interface ListAlert {
  id: string;
  message: string;
  sendToId: string;
  createdName: string;
  createdAvatar: string;
  createdEmail: string;
  createdBy: string;
  isRead: boolean;
  type: string;
  createdAt: Date;
}

interface NavigationBarProps {
  user?: UserInfo | null;
  navBarItems?: Array<{
    label: string;
    icon: React.ReactNode;
    action: () => void;
    className?: string;
  }>;
  toggleAskLogout?: () => void;
  openAskLogout?: boolean;
  shouldRenderAskLogout?: boolean;
  teacherMenuItems?: Array<{
    title: string;
    href: string;
    icon: React.ComponentType<React.SVGProps<SVGSVGElement>>;
    isAction?: boolean;
    subtitle?: string;
  }>;
}

const NavigationBar: React.FC<NavigationBarProps> = ({
  user,
  navBarItems,
  toggleAskLogout,
  openAskLogout,
  shouldRenderAskLogout,
  teacherMenuItems,
}) => {
  const { logout } = useAuth();
  const { t, i18n } = useTranslation('shared');
  const location = useLocation();
  const defaultData = useGet('dataAlert');
  const cachesFilterAlert = useGet('cachesFilterAlert');
  const [isTrigger, setIsTrigger] = useState(Boolean(!defaultData));
  const [notificationOpen, setNotificationOpen] = useState(false);
  const save = useSave();
  const [activeTab, setActiveTab] = useState('unread');
  const [
    openShowDetailNotification,
    toggleShowDetailNotification,
    shouldRenderShowDetailNotification,
  ] = useToggleDialog();
  const [alertId, setAlertId] = useState<string | null>(null);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  // Create SignalRService instance
  const signalRService = useMemo(() => createSignalRService(NOTIFY_HUB), []);

  const { filters } = useFiltersHandler({
    PageSize: cachesFilterAlert?.PageSize ?? 50,
    CurrentPage: cachesFilterAlert?.CurrentPage ?? 1,
    TextSearch: cachesFilterAlert?.TextSearch ?? '',
  });

  const stableFilters = useMemo(() => filters as IAlertRequest, [filters]);

  const {
    data: dataAlert,
    loading,
    refetch,
  } = useGetListAlert(stableFilters, {
    isTrigger: true,
    refetchKey: cachedKeys.refetchAlert,
    saveData: true,
  });

  useEffect(() => {
    if (dataAlert && isTrigger) {
      save(cachedKeys.dataAlert, dataAlert);
    }
  }, [dataAlert, isTrigger, save]);

  const dataMain: ListAlert[] = useMemo(
    () => (isTrigger ? dataAlert : defaultData),
    [dataAlert, defaultData, isTrigger],
  );

  // Filter notifications by read status
  const unreadNotifications = useMemo(() => {
    if (!dataMain) return [];
    return dataMain.filter((alert: ListAlert) => !alert.isRead);
  }, [dataMain]);

  const readNotifications = useMemo(() => {
    if (!dataMain) return [];
    return dataMain.filter((alert: ListAlert) => alert.isRead);
  }, [dataMain]);

  // Get unread notifications count
  const unreadCount = useMemo(() => {
    if (!dataMain || !Array.isArray(dataMain)) return 0;
    return dataMain.filter((alert: ListAlert) => !alert.isRead).length;
  }, [dataMain]);

  // Handle notification click (mark as read)
  const handleNotificationClick = async (alertId: string) => {
    try {
      await alertService.markAsRead(alertId);
      showSuccess('Notification marked as read');
      refetch();
      setNotificationOpen(false);
    } catch (error) {
      showError(error);
    }
  };

  // Handle deleting a single notification
  const handleDeleteAlert = async (alertId: string) => {
    try {
      await alertService.deleteAlert([alertId]);
      showSuccess('Notification deleted');
      refetch();
    } catch (error) {
      showError(error);
    }
  };

  // Handle deleting all notifications
  const handleDeleteAllAlerts = async () => {
    try {
      if (!dataMain || dataMain.length === 0) return;
      const allAlertIds = dataMain.map((alert: ListAlert) => alert.id);
      await alertService.deleteAlert(allAlertIds);
      showSuccess('All notifications deleted');
      refetch();
      setIsTrigger(true);
    } catch (error) {
      showError(error);
    }
  };

  // Handle marking all notifications as read
  const handleMarkAllAsRead = async () => {
    try {
      await alertService.markAllAsRead();
      showSuccess('All notifications marked as read');
      refetch();
      setNotificationOpen(false);
    } catch (error) {
      showError(error);
    }
  };

  // SignalR setup for receiving new notifications
  useEffect(() => {
    const initSignalR = async () => {
      try {
        connectionRef.current = await signalRService.start();
        signalRService.on('ReceiveNewNotification', (data: ListAlert) => {
          if (data.id && data.sendToId === user?.userID) {
            showSuccess(`Bạn có thông báo mới từ ${data.createdName}`);
            refetch();
          }
        });
      } catch (error) {
        console.error('Error in SignalR:', error);
      }
    };

    if (user?.userID) {
      initSignalR();
    }

    return () => {
      signalRService.stop();
    };
  }, [user?.userID, signalRService, refetch]);

  // Handle toggle for notification detail dialog
  const handleToggleShowDetailNotification = () => {
    toggleShowDetailNotification();
    if (openShowDetailNotification) {
      setAlertId(null);
    }
  };

  const filteredTeacherMenuItems = teacherMenuItems?.filter(
    (item) => item?.href === location.pathname,
  );

  return (
    <nav className="sticky top-0 z-50 w-full border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      {shouldRenderAskLogout && (
        <DialogConfirm
          isOpen={openAskLogout ?? false}
          content={t('Navigation.ConfirmLogout')}
          title={t('Logout')}
          toggle={toggleAskLogout ?? (() => {})}
          onSubmit={() => logout(user?.userID ?? '')}
          variantYes="destructive"
        />
      )}
      {shouldRenderShowDetailNotification && (
        <DialogDetailNotify
          isOpen={openShowDetailNotification}
          toggle={handleToggleShowDetailNotification}
          onSubmit={() => {
            handleNotificationClick(alertId ?? '');
            toggleShowDetailNotification();
          }}
          alertId={alertId ?? ''}
        />
      )}
      <div className="container-fluid mx-auto px-4">
        <div className="flex h-16 items-center justify-between">
          {/* Logo */}
          <div className="flex items-center space-x-4">
            {user?.roleId?.includes(ROLE_ENUM.Student) ? (
              <Link to="/" className="flex items-center space-x-2">
                <div className="relative h-12 overflow-hidden rounded-lg bg-gradient-to-br from-blue-500 to-purple-600">
                  <img
                    loading="lazy"
                    src={ImageSource.LogoFPT}
                    alt="Logo"
                    className="h-full w-full object-cover"
                  />
                </div>
              </Link>
            ) : (
              <div className="flex items-center space-x-2">
                {filteredTeacherMenuItems?.map(
                  (el) => el?.icon && <el.icon key={el?.title} className="h-8 w-8 text-primary" />,
                )}
                <div>
                  <h1 className="text-xl font-bold">
                    {filteredTeacherMenuItems?.map((el) => el?.title)}
                  </h1>
                  <p className="text-sm text-muted-foreground">
                    {filteredTeacherMenuItems?.map((el) => el?.subtitle)}
                  </p>
                </div>
              </div>
            )}
          </div>
          {/* Right Section */}
          <div className="flex items-center space-x-4">
            {/* Notifications */}
            <Popover open={notificationOpen} onOpenChange={setNotificationOpen}>
              <PopoverTrigger asChild>
                <Button variant="ghost" size="sm" className="relative h-9 w-9 p-0">
                  <Bell className="h-4 w-4" />
                  {unreadCount > 0 && (
                    <Badge
                      variant="destructive"
                      className="absolute -right-1 -top-1 flex h-5 w-5 items-center justify-center rounded-full p-0 text-[10px]"
                    >
                      {unreadCount > 99 ? '99+' : unreadCount}
                    </Badge>
                  )}
                </Button>
              </PopoverTrigger>
              <PopoverContent className="w-88 p-0" align="end">
                <div className="flex items-center justify-between border-b px-4 py-3">
                  <h4 className="font-semibold">Notifications</h4>
                  {unreadCount > 0 && (
                    <Badge variant="secondary" className="text-xs">
                      {unreadCount} new
                    </Badge>
                  )}
                </div>
                <Tabs value={activeTab} onValueChange={setActiveTab} className="w-full">
                  <TabsList className="grid w-full grid-cols-2 rounded-none border-b bg-transparent p-0">
                    <TabsTrigger
                      value="unread"
                      className="relative rounded-none border-b-2 border-transparent data-[state=active]:border-primary data-[state=active]:bg-transparent"
                    >
                      Chưa đọc
                      {unreadNotifications.length > 0 && (
                        <Badge
                          variant="destructive"
                          className="ml-1 flex h-5 w-5 min-w-[20px] items-center justify-center rounded-full p-0 text-[10px]"
                        >
                          {unreadNotifications.length > 99 ? '99+' : unreadNotifications.length}
                        </Badge>
                      )}
                    </TabsTrigger>
                    <TabsTrigger
                      value="read"
                      className="relative rounded-none border-b-2 border-transparent data-[state=active]:border-primary data-[state=active]:bg-transparent"
                    >
                      Đã đọc
                      {readNotifications.length > 0 && (
                        <Badge
                          variant="secondary"
                          className="ml-1 flex h-5 w-5 min-w-[20px] items-center justify-center rounded-full p-0 text-[10px]"
                        >
                          {readNotifications.length}
                        </Badge>
                      )}
                    </TabsTrigger>
                  </TabsList>
                  <TabsContent value="unread" className="m-0">
                    <ScrollArea className="h-80">
                      {loading ? (
                        <div className="flex items-center justify-center py-8">
                          <div className="text-sm text-muted-foreground">Loading...</div>
                        </div>
                      ) : unreadNotifications.length === 0 ? (
                        <div className="flex flex-col items-center justify-center py-8 text-center">
                          <MailOpen className="mb-2 h-8 w-8 text-muted-foreground" />
                          <div className="text-sm text-muted-foreground">
                            No unread notifications
                          </div>
                        </div>
                      ) : (
                        <div className="divide-y">
                          {unreadNotifications.map((alert: ListAlert) => (
                            <div
                              key={alert.id}
                              className="flex items-start space-x-3 bg-blue-50/50 p-4 transition-colors hover:bg-muted/50"
                            >
                              <button
                                type="button"
                                className="flex flex-1 cursor-pointer items-start space-x-3 text-left"
                                onClick={() => {
                                  toggleShowDetailNotification();
                                  setAlertId(alert.id);
                                }}
                              >
                                <Avatar className="h-8 w-8 flex-shrink-0">
                                  <AvatarImage src={alert.createdAvatar} alt={alert.createdName} />
                                  <AvatarFallback className="bg-gradient-to-br from-blue-500 to-purple-600 text-xs text-white">
                                    {alert.createdName.charAt(0).toUpperCase()}
                                  </AvatarFallback>
                                </Avatar>
                                <div className="min-w-0 flex-1">
                                  <div className="mb-1 flex items-center justify-between">
                                    <p className="truncate text-sm font-medium text-foreground">
                                      {alert.createdName}
                                    </p>
                                    <Mail className="h-3 w-3 text-blue-500" />
                                  </div>
                                  <p className="mb-1 line-clamp-2 text-sm text-muted-foreground">
                                    {alert.message}
                                  </p>
                                  <div className="flex items-center space-x-1 text-xs text-muted-foreground">
                                    <Clock className="h-3 w-3" />
                                    <span>{formatTimeAgo(alert.createdAt)}</span>
                                    {alert.type && (
                                      <>
                                        <span>•</span>
                                        <Badge variant="outline" className="px-1 py-0 text-xs">
                                          {alert.type}
                                        </Badge>
                                      </>
                                    )}
                                  </div>
                                </div>
                              </button>
                            </div>
                          ))}
                        </div>
                      )}
                    </ScrollArea>
                  </TabsContent>
                  <TabsContent value="read" className="m-0">
                    <ScrollArea className="h-80">
                      {loading ? (
                        <div className="flex items-center justify-center py-8">
                          <div className="text-sm text-muted-foreground">Loading...</div>
                        </div>
                      ) : readNotifications.length === 0 ? (
                        <div className="flex flex-col items-center justify-center py-8 text-center">
                          <Mail className="mb-2 h-8 w-8 text-muted-foreground" />
                          <div className="text-sm text-muted-foreground">No read notifications</div>
                        </div>
                      ) : (
                        <div className="divide-y">
                          {readNotifications.map((alert: ListAlert) => (
                            <div
                              key={alert.id}
                              className="flex items-start space-x-3 p-4 transition-colors hover:bg-muted/50"
                            >
                              <button
                                type="button"
                                className="flex flex-1 cursor-pointer items-start space-x-3 text-left"
                                onClick={() => {
                                  toggleShowDetailNotification();
                                  setAlertId(alert.id);
                                }}
                              >
                                <Avatar className="h-8 w-8 flex-shrink-0">
                                  <AvatarImage src={alert.createdAvatar} alt={alert.createdName} />
                                  <AvatarFallback className="bg-gradient-to-br from-blue-500 to-purple-600 text-xs text-white">
                                    {alert.createdName.charAt(0).toUpperCase()}
                                  </AvatarFallback>
                                </Avatar>
                                <div className="min-w-0 flex-1">
                                  <div className="mb-1 flex items-center justify-between">
                                    <p className="truncate text-sm font-medium text-foreground">
                                      {alert.createdName}
                                    </p>
                                    <MailOpen className="h-3 w-3 text-muted-foreground" />
                                  </div>
                                  <p className="mb-1 line-clamp-2 text-sm text-muted-foreground">
                                    {alert.message}
                                  </p>
                                  <div className="flex items-center space-x-1 text-xs text-muted-foreground">
                                    <Clock className="h-3 w-3" />
                                    <span>{formatTimeAgo(alert.createdAt)}</span>
                                    {alert.type && (
                                      <>
                                        <span>•</span>
                                        <Badge variant="outline" className="px-1 py-0 text-xs">
                                          {alert.type}
                                        </Badge>
                                      </>
                                    )}
                                  </div>
                                </div>
                              </button>
                              <Button
                                variant="ghost"
                                size="sm"
                                className="h-8 w-8 p-0"
                                onClick={() => handleDeleteAlert(alert.id)}
                              >
                                <Trash2 className="h-4 w-4 text-destructive" />
                              </Button>
                            </div>
                          ))}
                        </div>
                      )}
                    </ScrollArea>
                  </TabsContent>
                </Tabs>
                {dataMain && dataMain.length > 0 && (
                  <div className="flex justify-between border-t p-2">
                    {unreadCount > 0 && (
                      <Button
                        variant="ghost"
                        className="w-full text-sm"
                        size="sm"
                        onClick={handleMarkAllAsRead}
                      >
                        Đánh dấu tất cả đã đọc
                      </Button>
                    )}
                    {readNotifications.length > 0 && (
                      <Button
                        variant="ghost"
                        className="w-full text-sm"
                        size="sm"
                        onClick={handleDeleteAllAlerts}
                      >
                        <Trash2 className="mr-2 h-4 w-4 text-destructive" />
                        Xóa tất cả thông báo
                      </Button>
                    )}
                  </div>
                )}
              </PopoverContent>
            </Popover>
            {/* User Profile Dropdown */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" className="h-auto space-x-2 p-2">
                  <div className="hidden text-right sm:block">
                    <p className="text-sm font-medium leading-none">{user?.email}</p>
                  </div>
                  <Avatar className="h-8 w-8 ring-2 ring-primary/20">
                    <AvatarImage src={user?.avatarUrl ?? ''} alt={user?.fullName} />
                    <AvatarFallback className="bg-gradient-to-br from-blue-500 to-purple-600 text-white">
                      {user?.fullName}
                    </AvatarFallback>
                  </Avatar>
                  <ChevronDown className="h-4 w-4 text-muted-foreground" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent className="w-56" align="end">
                <DropdownMenuLabel className="font-semibold">{user?.fullName}</DropdownMenuLabel>
                <DropdownMenuSeparator />
                {navBarItems?.map((item, index) => (
                  <DropdownMenuItem key={index} onClick={item.action} className={item.className}>
                    {item.icon}
                    {item.label}
                  </DropdownMenuItem>
                ))}
                <DropdownMenuSeparator />
                <DropdownMenuItem
                  onClick={() => {
                    if (i18n.language === 'en') {
                      i18n.changeLanguage('vi');
                      localStorage.setItem(KEY_LANG, 'vi');
                    } else {
                      i18n.changeLanguage('en');
                      localStorage.setItem(KEY_LANG, 'en');
                    }
                  }}
                >
                  {t('Navigation.ChangeLanguage')} {i18n.language === 'en' ? '🇻🇳' : '🇺🇸'}
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </div>
      </div>
    </nav>
  );
};

export default React.memo(NavigationBar);
