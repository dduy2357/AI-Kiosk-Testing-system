import CommonIcons from '@/components/commonIcons';
import { addLeadingZero } from '@/helpers/common';
import { useCallback } from 'react';

interface PaginationProps {
  totalPage: number;
  currentPage: number;
  handleChangePage?: (value: number) => void;
}

const Pagination = (props: PaginationProps) => {
  //! State
  const { totalPage, currentPage, handleChangePage } = props;

  //! Function

  //! Render

  const renderPage = useCallback(() => {
    let pages = [];
    if (totalPage <= 7) {
      for (let i = 0; i < totalPage; i++) {
        pages.push(`${i + 1}`);
      }
    } else {
      pages.push('1');
      if (currentPage <= 3) {
        pages = ['1', '2', '3', '4', '...', `${totalPage}`];
      } else if (currentPage >= totalPage - 2) {
        pages = [
          '1',
          '...',
          `${totalPage - 3}`,
          `${totalPage - 2}`,
          `${totalPage - 1}`,
          `${totalPage}`,
        ];
      } else {
        pages = [
          '1',
          '...',
          `${currentPage - 1}`,
          `${currentPage}`,
          `${currentPage + 1}`,
          '...',
          `${totalPage}`,
        ];
      }
    }
    return pages.map((page, index) => {
      if (+page === currentPage) {
        return (
          <button
            key={`${page}-${index}`}
            onClick={() => {
              handleChangePage && handleChangePage(+page);
            }}
            className={'typo-7 text-main-primary mx-[5px] rounded px-2.5 py-[5px] hover:font-bold'}
          >
            {addLeadingZero(+page)}
          </button>
        );
      }

      return (
        <button
          key={`${page}-${index}`}
          className={
            'typo-7 mx-[5px] inline-block border-separate rounded px-2.5 py-[5px] hover:font-bold'
          }
          onClick={() => {
            if (page === '...') return;
            handleChangePage && handleChangePage(+page);
          }}
        >
          {page === '...' ? page : addLeadingZero(+page)}
        </button>
      );
    });
  }, [totalPage, currentPage, handleChangePage]);

  if (!totalPage) return null;

  return (
    <div className="flex items-center justify-center p-[15px]">
      <button
        onClick={() => {
          handleChangePage && handleChangePage(currentPage - 1);
        }}
        disabled={currentPage === 1}
      >
        <div>
          <CommonIcons.ArrowLeft
            className={currentPage !== 1 ? 'text-text-secondary' : 'text-disabled'}
          />
        </div>
      </button>
      {renderPage()}
      <button
        onClick={() => {
          handleChangePage && handleChangePage(currentPage + 1);
        }}
        disabled={currentPage === totalPage}
      >
        <div>
          <CommonIcons.ArrowRight
            className={currentPage !== totalPage ? 'text-text-secondary' : 'text-disabled'}
          />
        </div>
      </button>
    </div>
  );
};

export default Pagination;
