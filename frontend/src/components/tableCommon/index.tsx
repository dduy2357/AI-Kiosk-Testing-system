import { get } from 'lodash';
import * as React from 'react';
import {
  Table,
  TableCaption,
  TableHead,
  TableHeader,
  TableRow,
  TableBody,
  TableCell,
} from '../ui/table';
import ScrollWrapper from '../ui/ScrollWrapper';

interface TableProps<T> {
  data: T[];
  columns: {
    label: string;
    accessor: string;
    width?: number;
    Cell?: (row: T) => React.ReactNode;
  }[];
  tableCaption?: string;
  onClickRow?: (rowData: T) => void;
  keyRow?: string;
  className?: string;
  currentPage?: number;
  handleChangePage?: (value: number) => void;
  noResultText?: string;
  loadingMore?: boolean;
  hasMore?: boolean;
}
const TableCommon = <T,>(props: TableProps<T>) => {
  const {
    columns,
    data,
    className,
    keyRow = 'id',
    tableCaption,
    onClickRow,
    currentPage,
    noResultText = 'No Result',
    handleChangePage,
    loadingMore,
    hasMore,
  } = props;

  return (
    <>
      <Table className={`${className} border-collapse`}>
        {tableCaption && <TableCaption>{tableCaption}</TableCaption>}
        <TableHeader>
          <TableRow className="border-none">
            {columns.map((col) => {
              return (
                <TableHead
                  key={col.accessor}
                  align="center"
                  className={'typo-7 font-medium text-black'}
                >
                  {col.label}
                </TableHead>
              );
            })}
          </TableRow>
        </TableHeader>
        <TableBody>
          <ScrollWrapper
            key={`${loadingMore}`}
            onScrollEnd={() => {
              if (hasMore && !loadingMore) {
                handleChangePage?.((currentPage ?? 1) + 1);
              }
            }}
          >
            {data.map((row) => {
              return (
                <TableRow
                  key={get(row, keyRow)}
                  className="border-none hover:bg-white"
                  onClick={() => onClickRow && onClickRow(row)}
                >
                  {columns.map((col, index) => {
                    if (index === 0) {
                      return (
                        <TableCell
                          key={`${get(row, keyRow)}-${index}`}
                          className={`typo-7 rounded-bl-xl rounded-tl-xl`}
                          style={{ width: col?.width ?? 'unset' }}
                        >
                          {col.Cell ? col.Cell(row) : get(row, col.accessor)}
                        </TableCell>
                      );
                    }
                    if (index === columns.length - 1) {
                      return (
                        <TableCell
                          key={`${get(row, keyRow)}-${index}`}
                          style={{ width: col?.width ?? 'unset' }}
                          className={`typo-7 rounded-br-xl rounded-tr-xl`}
                        >
                          {col.Cell ? col.Cell(row) : get(row, col.accessor)}
                        </TableCell>
                      );
                    }
                    return (
                      <TableCell
                        key={`${get(row, keyRow)}-${index}`}
                        className={'typo-7'}
                        style={{ width: col?.width ?? 'unset' }}
                      >
                        {col.Cell ? col.Cell(row) : get(row, col.accessor)}
                      </TableCell>
                    );
                  })}
                </TableRow>
              );
            })}
          </ScrollWrapper>
        </TableBody>
      </Table>
      {data?.length === 0 && <div className={`typo-7 w-full p-10 text-center`}>{noResultText}</div>}
    </>
  );
};

export default TableCommon;
