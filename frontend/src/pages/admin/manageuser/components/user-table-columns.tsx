import {
  BookOpen,
  Briefcase,
  Crown,
  Edit,
  Eye,
  GraduationCap,
  KeyRound,
  Mail,
  MapPin,
  MoreHorizontal,
  User,
  Users,
  UserX,
} from 'lucide-react';

import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '@/components/ui/tooltip';
import { PASSWORD_AFTER_RESET } from '@/consts/common';

interface UserList {
  userId: string;
  fullName: string;
  userCode: any;
  phone: string;
  email: string;
  avatarUrl: string;
  sex: any;
  isActive: boolean;
  createAt: string;
  updateAt: any;
  createUser: string;
  updateUser: any;
  status: number;
  lastLogin: string;
  lastLoginIp: string;
  dob: string;
  address: any;
  campusId?: string;
  departmentId: any;
  positionId: any;
  majorId: any;
  specializationId: any;
  roleId: any[];
  position: string;
  major: string;
  campus: string;
  department: string;
  specialization: string;
}

interface ColumnProps {
  navigate: (path: string) => void;
  BaseUrl: { AdminAddNewUser: string };
  onViewUser?: (user: UserList) => void;
  onDeactivateUser?: (userId: string) => Promise<void>;
  onResetPassword: (userId: string, newPass: string) => Promise<void>;
}

export const getRoleInfo = (roleId: number) => {
  const roleMap = {
    1: {
      name: 'Student',
      color: 'bg-blue-100 text-blue-800 dark:bg-blue-900/20 dark:text-blue-400',
      icon: GraduationCap,
    },
    2: {
      name: 'Lecturer',
      color: 'bg-purple-100 text-purple-800 dark:bg-purple-900/20 dark:text-purple-400',
      icon: BookOpen,
    },
    3: {
      name: 'Supervisor',
      color: 'bg-orange-100 text-orange-800 dark:bg-orange-900/20 dark:text-orange-400',
      icon: Users,
    },
    4: {
      name: 'Admin',
      color: 'bg-red-100 text-red-800 dark:bg-red-900/20 dark:text-red-400',
      icon: Crown,
    },
  };

  return (
    roleMap[roleId as keyof typeof roleMap] || {
      name: 'N/A',
      color: 'bg-gray-100 text-gray-800',
      icon: User,
    }
  );
};

const getInitials = (name: string) => {
  return name
    .split(' ')
    .map((word) => word.charAt(0))
    .join('')
    .toUpperCase()
    .slice(0, 2);
};

export const createUserColumns = ({
  navigate,
  BaseUrl,
  onViewUser,
  onDeactivateUser,
  onResetPassword,
}: ColumnProps) => [
  {
    label: 'User',
    accessor: 'fullName',
    width: 180,
    sortable: false,
    Cell: (row: UserList) => (
      <div className="flex items-center space-x-2 py-1">
        <Avatar className="h-8 w-8">
          <AvatarImage src={row.avatarUrl || ''} alt={row.fullName} />
          <AvatarFallback className="bg-gradient-to-br from-indigo-500 to-purple-600 text-xs font-semibold text-white">
            {getInitials(row.fullName)}
          </AvatarFallback>
        </Avatar>
        <div className="flex min-w-0 flex-col">
          <span className="truncate text-sm font-medium text-gray-900 dark:text-gray-100">
            {row.fullName}
          </span>
          <span className="truncate text-xs text-gray-500 dark:text-gray-400">
            {row.userCode ? `ID: ${row.userCode}` : 'ID: N/A'}
          </span>
        </div>
      </div>
    ),
  },
  {
    label: 'Role & Work',
    accessor: 'roleAndWork',
    width: 200,
    sortable: false,
    Cell: (row: UserList) => {
      const roles = row.roleId || [];
      const workInfo = [
        row.position && `Chức vụ: ${row.position}`,
        row.major && `Chuyên ngành: ${row.major}`,
        row.department && `Phòng ban: ${row.department}`,
        row.specialization && `Chuyên môn: ${row.specialization}`,
      ].filter(Boolean);

      const allInfo = [
        roles.length > 0 && `Vai trò: ${roles.map((id) => getRoleInfo(id).name).join(', ')}`,
        ...workInfo,
      ].filter(Boolean);

      return (
        <TooltipProvider>
          <Tooltip>
            <TooltipTrigger>
              <div className="flex items-center space-x-1">
                <div className="flex h-6 w-6 items-center justify-center rounded bg-amber-100 dark:bg-amber-900/20">
                  <Briefcase className="h-3 w-3 text-amber-600 dark:text-amber-400" />
                </div>
                <div className="flex min-w-0 flex-col">
                  {roles.length > 0 ? (
                    (() => {
                      const roleInfo = getRoleInfo(roles[0]);
                      const RoleIcon = roleInfo.icon;
                      return (
                        <Badge className={`${roleInfo.color} border-0 px-1.5 py-0.5 text-xs`}>
                          <RoleIcon className="mr-1 h-2.5 w-2.5" />
                          {roleInfo.name}
                        </Badge>
                      );
                    })()
                  ) : (
                    <Badge variant="outline" className="text-xs text-gray-500">
                      <User className="mr-1 h-2.5 w-2.5" />
                      N/A
                    </Badge>
                  )}
                  {workInfo.length > 0 && (
                    <span className="truncate text-xs text-gray-500 dark:text-gray-400">
                      {workInfo[0]}
                    </span>
                  )}
                </div>
              </div>
            </TooltipTrigger>
            {allInfo.length > 0 && (
              <TooltipContent>
                <div className="space-y-1">
                  {allInfo.map((info, index) => (
                    <div key={index} className="text-xs">
                      {info}
                    </div>
                  ))}
                </div>
              </TooltipContent>
            )}
          </Tooltip>
        </TooltipProvider>
      );
    },
  },
  {
    label: 'Contact',
    accessor: 'contact',
    width: 160,
    sortable: false,
    Cell: (row: UserList) => {
      const contactInfo = [
        row.email && `Email: ${row.email}`,
        row.phone && `Phone: ${row.phone}`,
      ].filter(Boolean);

      return (
        <TooltipProvider>
          <Tooltip>
            <TooltipTrigger>
              <div className="flex items-center space-x-1">
                <div className="flex h-6 w-6 items-center justify-center rounded bg-red-100 dark:bg-red-900/20">
                  <Mail className="h-3 w-3 text-red-600 dark:text-red-400" />
                </div>
                <span className="truncate text-xs text-blue-600 hover:text-blue-800 hover:underline dark:text-blue-400 dark:hover:text-blue-300">
                  {contactInfo.length > 0 ? (
                    contactInfo[0]?.split(': ')[1]
                  ) : (
                    <span className="italic text-gray-400">N/A</span>
                  )}
                </span>
              </div>
            </TooltipTrigger>
            {contactInfo.length > 0 && (
              <TooltipContent>
                <div className="space-y-1">
                  {contactInfo.map((info, index) => (
                    <div key={index} className="text-xs">
                      {info}
                    </div>
                  ))}
                </div>
              </TooltipContent>
            )}
          </Tooltip>
        </TooltipProvider>
      );
    },
  },
  {
    label: 'Campus',
    accessor: 'campus',
    width: 100,
    sortable: false,
    Cell: (row: UserList) => (
      <div className="flex items-center space-x-1">
        <div className="flex h-6 w-6 items-center justify-center rounded bg-teal-100 dark:bg-teal-900/20">
          <MapPin className="h-3 w-3 text-teal-600 dark:text-teal-400" />
        </div>
        <span className="truncate text-xs text-gray-900 dark:text-gray-100">
          {row.campus || <span className="italic text-gray-400">N/A</span>}
        </span>
      </div>
    ),
  },
  {
    label: 'Status',
    accessor: 'status',
    width: 100,
    sortable: false,
    Cell: (row: UserList) => (
      <div className="">
        <Badge
          variant={row.status === 1 ? 'default' : 'secondary'}
          className={`rounded-full border-0 px-2 py-0.5 text-xs font-medium shadow-sm ${
            row.status === 1
              ? 'bg-emerald-100 text-emerald-800 dark:bg-emerald-900/20 dark:text-emerald-400'
              : 'bg-red-100 text-red-800 dark:bg-red-900/20 dark:text-red-400'
          }`}
        >
          <div
            className={`mr-1 h-1.5 w-1.5 rounded-full ${row.status === 1 ? 'bg-emerald-500' : 'bg-red-500'}`}
          />
          {row.status === 1 ? 'Activate' : 'Deactivate'}
        </Badge>
      </div>
    ),
  },
  {
    label: 'Actions',
    accessor: 'actions',
    width: 80,
    sortable: false,
    Cell: (row: UserList) => (
      <div className="flex items-center justify-center">
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button
              variant="ghost"
              className="h-7 w-7 rounded p-0 transition-colors hover:bg-gray-100 dark:hover:bg-gray-800"
            >
              <MoreHorizontal className="h-3.5 w-3.5 text-gray-600 dark:text-gray-400" />
              <span className="sr-only">Open actions menu</span>
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-40">
            {onViewUser && (
              <DropdownMenuItem
                onClick={() => onViewUser(row)}
                className="cursor-pointer text-xs hover:bg-gray-50 dark:hover:bg-gray-800"
              >
                <Eye className="mr-2 h-3.5 w-3.5 text-blue-500" />
                <span>View Details</span>
              </DropdownMenuItem>
            )}
            <DropdownMenuItem
              onClick={() => navigate(`${BaseUrl.AdminAddNewUser}/${row.userId}`)}
              className="cursor-pointer text-xs hover:bg-gray-50 dark:hover:bg-gray-800"
            >
              <Edit className="mr-2 h-3.5 w-3.5 text-amber-500" />
              <span>Edit</span>
            </DropdownMenuItem>
            <DropdownMenuItem
              onClick={() => onResetPassword(row.userId, PASSWORD_AFTER_RESET)}
              className="cursor-pointer text-xs hover:bg-gray-50 dark:hover:bg-gray-800"
            >
              <KeyRound className="mr-2 h-3.5 w-3.5 text-amber-500" />
              <span>Reset Password</span>
            </DropdownMenuItem>
            {onDeactivateUser && (
              <>
                <DropdownMenuSeparator />
                <DropdownMenuItem
                  onClick={() => onDeactivateUser(row?.userId)}
                  className={`cursor-pointer text-xs ${
                    row?.status === 1
                      ? 'text-red-600 hover:bg-red-50 focus:bg-red-50 dark:hover:bg-red-900/20 dark:focus:bg-red-900/20'
                      : 'text-green-600 hover:bg-green-50 focus:bg-green-50 dark:hover:bg-green-900/20 dark:focus:bg-green-900/20'
                  }`}
                >
                  {row?.status === 1 ? (
                    <UserX className="mr-2 h-3.5 w-3.5" />
                  ) : (
                    <User className="mr-2 h-3.5 w-3.5" />
                  )}
                  <span>{row?.status === 1 ? 'Deactivate' : 'Reactivate'}</span>
                </DropdownMenuItem>
              </>
            )}
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    ),
  },
];
