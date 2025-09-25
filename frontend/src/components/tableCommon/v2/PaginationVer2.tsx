import { Badge } from '@/components/ui/badge';
import { Card } from '@/components/ui/card';
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { ChevronLeft, ChevronRight, ChevronsLeft, ChevronsRight } from 'lucide-react';
import React from 'react';
import { useTranslation } from 'react-i18next';
import NavigationButton from './NavigationButton';

export interface IPaginationVersionTop {
  currentPage?: number;
  totalPage?: number;
  rowPerPage?: number;
  listOptions?: number[];
  onChangePage?: (page?: number) => void;
  onPageSizeChange?: (size?: number) => void;
  dataLength?: number;
  total?: number;
  compact?: boolean;
}

const listOptionsDefault = [5, 10, 20, 50, 100];

const PaginationVersionTop = (props: IPaginationVersionTop) => {
  const {
    currentPage = 1,
    totalPage = 1,
    rowPerPage = 10,
    listOptions = listOptionsDefault,
    onChangePage,
    onPageSizeChange,
    dataLength = 0,
    total = 0,
    compact = false,
  } = props;
  const { t } = useTranslation('shared');

  const startItem = (currentPage - 1) * rowPerPage + 1;
  const endItem = (currentPage - 1) * rowPerPage + dataLength;

  if (compact) {
    return (
      <Card className="border-gray-200 bg-white/80 p-3 shadow-sm backdrop-blur-sm">
        <div className="flex items-center justify-between gap-4">
          <div className="flex items-center gap-2 text-sm text-gray-600">
            <span className="font-medium text-gray-900">
              {startItem}-{endItem}
            </span>
            <span>của</span>
            <Badge variant="secondary" className="bg-blue-100 text-blue-800">
              {total}
            </Badge>
          </div>

          <div className="flex items-center gap-1">
            <NavigationButton
              onClick={() => onChangePage?.(currentPage - 1)}
              disabled={currentPage === 1}
              icon={ChevronLeft}
              label="Trang trước"
            />

            <div className="rounded-md bg-gray-50 px-3 py-1 text-sm font-medium text-gray-700">
              {currentPage}/{totalPage}
            </div>

            <NavigationButton
              onClick={() => onChangePage?.(currentPage + 1)}
              disabled={currentPage === totalPage}
              icon={ChevronRight}
              label="Trang sau"
            />
          </div>
        </div>
      </Card>
    );
  }

  return (
    <Card className="border-gray-200 bg-gradient-to-r from-white/90 to-gray-50/90 p-4 shadow-lg backdrop-blur-sm">
      <div className="flex flex-col items-center justify-between gap-4 lg:flex-row">
        {/* Left side - Info and Row selector */}
        <div className="flex flex-col items-center gap-4 sm:flex-row">
          <div className="flex items-center gap-2 text-sm text-gray-600">
            <span>{t('Display')}</span>
            <span className="font-semibold text-gray-900">
              {startItem}-{endItem}
            </span>
            <span>{t('In')}</span>
            <Badge variant="secondary" className="bg-blue-100 font-semibold text-blue-800">
              {total} {t('Items')}
            </Badge>
          </div>

          <div className="flex items-center gap-2">
            <span className="whitespace-nowrap text-sm text-gray-600">{t('NumberOfRows')}:</span>
            <Select
              value={`${rowPerPage}`}
              onValueChange={(value: string) => onPageSizeChange?.(Number(value))}
            >
              <SelectTrigger className="h-9 w-20 border-gray-200 bg-white transition-colors hover:border-blue-300 focus:border-blue-500">
                <SelectValue />
              </SelectTrigger>
              <SelectContent className="border-gray-200 bg-white shadow-xl">
                <SelectGroup>
                  {listOptions?.map((option, index) => (
                    <SelectItem
                      key={index}
                      value={`${option}`}
                      className="hover:bg-blue-50 focus:bg-blue-50"
                    >
                      {option}
                    </SelectItem>
                  ))}
                </SelectGroup>
              </SelectContent>
            </Select>
          </div>
        </div>

        {/* Right side - Navigation */}
        <div className="flex items-center gap-2">
          {/* First page button */}
          <NavigationButton
            onClick={() => onChangePage?.(1)}
            disabled={currentPage === 1}
            icon={ChevronsLeft}
            label="Trang đầu"
          />

          {/* Previous page button */}
          <NavigationButton
            onClick={() => onChangePage?.(currentPage - 1)}
            disabled={currentPage === 1}
            icon={ChevronLeft}
            label="Trang trước"
          />

          {/* Page numbers */}
          <div className={'text-text-sub flex'}>
            {currentPage && totalPage && (
              <>
                <span className="typo-30 2xl:text-typo-30 text-black">{currentPage}</span>
                <span className="typo-30 2xl:text-typo-30">/</span>
                <span className="typo-30 2xl:text-typo-30 text-text-sub">{totalPage}</span>
              </>
            )}
          </div>

          {/* Current page indicator for mobile */}
          <div className="rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm font-medium text-gray-700 sm:hidden">
            {currentPage}/{totalPage}
          </div>

          {/* Next page button */}
          <NavigationButton
            onClick={() => onChangePage?.(currentPage + 1)}
            disabled={currentPage === totalPage}
            icon={ChevronRight}
            label={t('NextPage')}
          />

          {/* Last page button */}
          <NavigationButton
            onClick={() => onChangePage?.(totalPage)}
            disabled={currentPage === totalPage}
            icon={ChevronsRight}
            label={t('LastPage')}
          />
        </div>
      </div>
    </Card>
  );
};

export default React.memo(PaginationVersionTop);
