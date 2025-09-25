import {
  DropdownMenu,
  DropdownMenuTrigger,
  DropdownMenuContent,
  DropdownMenuItem,
} from '@/components/ui/dropdown-menu';
import { Button } from '@/components/ui/button';
import { MoreHorizontal, Edit, Trash } from 'lucide-react';
import { useTranslation } from 'react-i18next';

interface PermissionItem {
  id: string;
  name: string;
  action: string;
}

interface PermissionCellContentProps {
  permission: PermissionItem;
  onEdit: (id: string) => void;
  onDelete: (id: string) => void;
}

export function PermissionCellContent({
  permission,
  onEdit,
  onDelete,
}: Readonly<PermissionCellContentProps>) {
  const { t } = useTranslation('shared');

  return (
    <div className="inline-flex items-center gap-1 rounded-full bg-gray-100 px-2.5 py-0.5 text-sm font-medium text-gray-800 dark:bg-gray-800 dark:text-gray-200">
      <span>{permission.name}</span>
      <span className="text-xs text-gray-500 dark:text-gray-400">({permission.action})</span>
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button variant="ghost" className="ml-1 h-6 w-6 p-0">
            <MoreHorizontal className="h-3.5 w-3.5" />
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end">
          <DropdownMenuItem
            onClick={() => onEdit(permission.id)}
            className="cursor-pointer text-blue-600 hover:bg-blue-100"
          >
            <Edit className="mr-2 h-4 w-4" color="blue" /> {t('Edit')}
          </DropdownMenuItem>
          <DropdownMenuItem
            onClick={() => onDelete(permission.id)}
            className="cursor-pointer text-red-600 hover:bg-red-100"
          >
            <Trash className="mr-2 h-4 w-4" color="red" /> {t('Delete')}
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
    </div>
  );
}
