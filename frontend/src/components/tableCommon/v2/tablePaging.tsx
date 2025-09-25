import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import { AnimatePresence, motion } from 'framer-motion';
import { get } from 'lodash';
import { ArrowUpDown, ChevronDown, ChevronUp, Database, Sparkles } from 'lucide-react';
import * as React from 'react';
import PaginationVersionTop from './PaginationVer2';
import { useTranslation } from 'react-i18next';

export enum ORDER_BY {
  DESC = 'desc',
  ASC = 'asc',
}

interface TableProps<T> {
  data: T[];
  columns: {
    label: string;
    accessor: string;
    width?: number;
    headerClassName?: string;
    Cell?: (row: T) => React.ReactNode;
    sortable?: boolean;
  }[];
  onClickRow?: (rowData: T) => void;
  keyRow?: string;
  className?: string;
  currentPage?: number;
  currentSize?: number;
  totalPage?: number;
  total?: number;
  handleChangePage?: (value: number) => void;
  noResultText?: string;
  handleChangeSize?: (value: number) => void;
  classPagination?: string;
  classHeadAndCell?: string;
  orderBy?: string;
  sortBy?: ORDER_BY;
  onChangeSortBy?: (newSort: ORDER_BY) => void;
  onChangeOrderBy?: (newOrder: string) => void;
  loading?: boolean;
  title?: string;
  description?: string;
}

const TablePaging = <T,>(props: TableProps<T>) => {
  const {
    columns,
    data,
    className,
    keyRow,
    onClickRow,
    currentSize,
    currentPage,
    totalPage = 1,
    total,
    noResultText = 'Không tìm thấy dữ liệu',
    handleChangePage,
    handleChangeSize,
    classPagination,
    classHeadAndCell,
    orderBy,
    sortBy,
    onChangeSortBy,
    onChangeOrderBy,
    loading = false,
    title,
    description,
  } = props;
  const { t } = useTranslation('shared');
  const [hoveredRow, setHoveredRow] = React.useState<string | null>(null);

  const handleChangeOrderBy = (newAccessor: string) => {
    if (newAccessor !== orderBy) {
      onChangeOrderBy?.(newAccessor);
      onChangeSortBy?.(ORDER_BY.ASC);
    } else {
      onChangeSortBy?.(sortBy === ORDER_BY.ASC ? ORDER_BY.DESC : ORDER_BY.ASC);
    }
  };

  const getSortIcon = (accessor: string) => {
    if (orderBy !== accessor) {
      return (
        <ArrowUpDown className="h-4 w-4 text-gray-400 transition-colors group-hover:text-gray-600" />
      );
    }

    return (
      <motion.div initial={{ scale: 0.8 }} animate={{ scale: 1 }} className="flex flex-col">
        <ChevronUp
          className={`h-3 w-3 transition-colors ${
            sortBy === ORDER_BY.ASC ? 'text-blue-600' : 'text-gray-300'
          }`}
        />
        <ChevronDown
          className={`-mt-1 h-3 w-3 transition-colors ${
            sortBy === ORDER_BY.DESC ? 'text-blue-600' : 'text-gray-300'
          }`}
        />
      </motion.div>
    );
  };

  const LoadingSkeleton = () => (
    <>
      {[...Array(currentSize ?? 5)].map((_, index) => (
        <tr key={`skeleton-${index}`} className="border-b border-gray-100">
          {columns.map((_, colIndex) => (
            <td key={`skeleton-${index}-${colIndex}`} className="px-6 py-4">
              <div className="h-4 animate-pulse rounded bg-gradient-to-r from-gray-200 via-gray-100 to-gray-200" />
            </td>
          ))}
        </tr>
      ))}
    </>
  );

  const EmptyState = () => (
    <tr>
      <td colSpan={columns.length} className="px-6 py-16">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="flex flex-col items-center gap-4"
        >
          <div className="relative">
            <div className="rounded-full bg-gradient-to-br from-blue-50 to-indigo-100 p-6">
              <Database className="h-12 w-12 text-blue-500" />
            </div>
            <motion.div
              animate={{ rotate: 360 }}
              transition={{
                duration: 2,
                repeat: Number.POSITIVE_INFINITY,
                ease: 'linear',
              }}
              className="absolute -right-1 -top-1"
            >
              <Sparkles className="h-6 w-6 text-yellow-500" />
            </motion.div>
          </div>
          <div className="space-y-2 text-center">
            <h3 className="text-lg font-semibold text-gray-900">{t('NoDataFound')}</h3>
            <p className="max-w-sm text-sm text-gray-500">
              {noResultText || t('NoDataFoundDescription')}
            </p>
          </div>
        </motion.div>
      </td>
    </tr>
  );

  return (
    <div className="space-y-6">
      {(title || description) && (
        <div className="space-y-2">
          {title && (
            <div className="flex items-center gap-2">
              <h2 className="text-2xl font-bold text-gray-900">{title}</h2>
              {total && (
                <Badge variant="secondary" className="bg-blue-100 text-blue-800">
                  {total} mục
                </Badge>
              )}
            </div>
          )}
          {description && <p className="text-gray-600">{description}</p>}
        </div>
      )}

      <Card className="overflow-hidden border-0 bg-white/80 shadow-lg backdrop-blur-sm">
        <CardContent className="p-0">
          <div className={`${className} overflow-x-auto`}>
            <table className="w-full">
              <thead>
                <tr className="border-b border-gray-200 bg-gradient-to-r from-gray-50 to-gray-100/50">
                  {columns.map((col, index) => (
                    <th
                      key={col?.accessor || index}
                      className={`px-6 py-5 text-left ${classHeadAndCell}`}
                      style={{ width: col?.width }}
                    >
                      <button
                        type="button"
                        className={`group flex items-center gap-3 ${
                          col.sortable !== false ? 'cursor-pointer' : ''
                        }`}
                        onClick={() => col.sortable !== false && handleChangeOrderBy(col?.accessor)}
                      >
                        <span className="text-sm font-semibold text-gray-700 transition-colors group-hover:text-gray-900">
                          {col?.label}
                        </span>
                        {col.sortable !== false && getSortIcon(col.accessor)}
                      </button>
                    </th>
                  ))}
                </tr>
              </thead>

              <tbody className="divide-y divide-gray-100">
                <AnimatePresence>
                  {loading ? (
                    <LoadingSkeleton />
                  ) : data.length === 0 ? (
                    <EmptyState />
                  ) : (
                    data.map((row, rowIndex) => {
                      // Use keyRow if provided and valid, otherwise fallback to rowIndex
                      const rowKey =
                        keyRow && get(row, keyRow) ? String(get(row, keyRow)) : `row-${rowIndex}`;
                      return (
                        <motion.tr
                          key={rowKey}
                          initial={{ opacity: 0, x: -20 }}
                          animate={{ opacity: 1, x: 0 }}
                          exit={{ opacity: 0, x: 20 }}
                          transition={{
                            duration: 0.3,
                            delay: rowIndex * 0.03,
                            ease: 'easeOut',
                          }}
                          onClick={() => onClickRow?.(row)}
                          onMouseEnter={() => setHoveredRow(rowKey)}
                          onMouseLeave={() => setHoveredRow(null)}
                          className={`group cursor-pointer transition-all duration-200 ease-out ${
                            hoveredRow === rowKey
                              ? 'scale-[1.01] transform bg-gradient-to-r from-blue-50/80 to-indigo-50/40 shadow-sm'
                              : 'bg-white hover:bg-gray-50/50'
                          } `}
                        >
                          {columns.map((col, colIndex) => (
                            <td
                              key={`${rowKey}-${col.accessor}-${colIndex}`}
                              className={`px-6 py-4 transition-all duration-200 ${classHeadAndCell}`}
                              style={{ width: col?.width }}
                            >
                              <div
                                className={`text-sm transition-all duration-200 ${
                                  hoveredRow === rowKey
                                    ? 'font-medium text-gray-900'
                                    : 'text-gray-700'
                                } `}
                              >
                                {col.Cell ? col.Cell(row) : get(row, col.accessor)}
                              </div>
                            </td>
                          ))}
                        </motion.tr>
                      );
                    })
                  )}
                </AnimatePresence>
              </tbody>
            </table>
          </div>
        </CardContent>

        {currentPage && (
          <div className={`${classPagination}`}>
            <PaginationVersionTop
              currentPage={currentPage}
              rowPerPage={currentSize}
              totalPage={totalPage}
              dataLength={data?.length}
              total={total}
              onPageSizeChange={(size: any) => {
                handleChangeSize?.(size ?? 10);
              }}
              onChangePage={(page?: number) => {
                handleChangePage?.(page ?? 1);
              }}
            />
          </div>
        )}
      </Card>
    </div>
  );
};

const MemoizedTablePaging = React.memo(TablePaging) as typeof TablePaging;
export default MemoizedTablePaging;
