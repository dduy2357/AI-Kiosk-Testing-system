import { cn } from '@/lib/utils';

interface ExamHeaderProps {
  title?: string;
  subtitle?: string;
  icon?: React.ReactNode;
  className?: string;
}

const ExamHeader = ({ title, subtitle, icon, className }: ExamHeaderProps) => {
  return (
    <div className={cn('bg-blue-600 px-6 py-6', className)}>
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          {icon && <div className="rounded-xl bg-white/20 p-3">{icon}</div>}
          <div>
            <h1 className="text-3xl font-bold text-white">{title || 'Loading...'}</h1>
            {subtitle && <p className="text-blue-100">{subtitle}</p>}
          </div>
        </div>
      </div>
    </div>
  );
};

export default ExamHeader;
