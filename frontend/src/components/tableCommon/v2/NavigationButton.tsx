import { Button } from '@/components/ui/button';
import { motion } from 'framer-motion';
import { twMerge } from 'tailwind-merge';

const NavigationButton = ({
  onClick,
  disabled,
  icon: Icon,
  label,
  variant = 'outline',
}: {
  onClick: () => void;
  disabled: boolean;
  icon: any;
  label: string;
  variant?: 'outline' | 'ghost';
}) => (
  <motion.div whileHover={{ scale: disabled ? 1 : 1.05 }} whileTap={{ scale: disabled ? 1 : 0.95 }}>
    <Button
      variant={variant}
      size="sm"
      onClick={onClick}
      disabled={disabled}
      className={twMerge(
        'h-9 w-9 p-0 transition-all duration-200',
        disabled
          ? 'cursor-not-allowed opacity-40'
          : 'shadow-sm hover:border-blue-200 hover:bg-blue-50 hover:text-blue-600',
      )}
      aria-label={label}
    >
      <Icon className="h-4 w-4" />
    </Button>
  </motion.div>
);

export default NavigationButton;
