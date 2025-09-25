import * as React from 'react';
import { cn } from '@/lib/utils';

export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  extraRight?: React.ReactNode;
  extraLeft?: React.ReactNode;
  iconText?: string;
  isIcon?: boolean;
}

const Input = React.forwardRef<HTMLInputElement, InputProps>(
  ({ className, type, extraRight, extraLeft, iconText, isIcon, ...props }, ref) => {
    return (
      <div className="relative w-full">
        {extraLeft && <div className="absolute left-3 top-1/2 -translate-y-1/2">{extraLeft}</div>}
        <input
          type={type}
          className={cn(
            'flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50',
            extraLeft && 'pl-10',
            extraRight && 'pr-10',
            className,
          )}
          ref={ref}
          {...props}
        />
        {extraRight && (
          <div className="absolute right-3 top-1/2 -translate-y-1/2 cursor-pointer">
            {extraRight}
          </div>
        )}
        {isIcon && <div className="absolute bottom-3 right-3.5 2xl:bottom-1.5">{iconText}</div>}
      </div>
    );
  },
);

Input.displayName = 'Input';

export { Input };
