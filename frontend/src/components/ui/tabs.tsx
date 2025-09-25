import * as React from "react";
import * as TabsPrimitive from "@radix-ui/react-tabs";
import { motion } from "framer-motion";

import { cn } from "@/lib/utils";

const Tabs = TabsPrimitive.Root;

interface TabsListProps
  extends React.ComponentPropsWithoutRef<typeof TabsPrimitive.List> {
  variant?: "default" | "pills" | "underlined" | "boxed" | "gradient";
  size?: "sm" | "md" | "lg";
  fullWidth?: boolean;
}

const TabsList = React.forwardRef<
  React.ElementRef<typeof TabsPrimitive.List>,
  TabsListProps
>(
  (
    {
      className,
      variant = "default",
      size = "md",
      fullWidth = false,
      ...props
    },
    ref,
  ) => {
    const variantStyles = {
      default:
        "bg-muted rounded-md p-1 shadow-sm border border-transparent dark:border-border/10",
      pills: "bg-transparent gap-2",
      underlined:
        "bg-transparent border-b border-border/30 rounded-none p-0 gap-4",
      boxed: "bg-transparent gap-1",
      gradient:
        "bg-gradient-to-br from-background/80 to-muted/80 backdrop-blur-sm rounded-lg p-1 shadow-md border border-border/10",
    };

    const sizeStyles = {
      sm: "h-8",
      md: "h-10",
      lg: "h-12",
    };

    return (
      <TabsPrimitive.List
        ref={ref}
        className={cn(
          "inline-flex items-center justify-center text-muted-foreground",
          variantStyles[variant],
          sizeStyles[size],
          fullWidth && "w-full",
          className,
        )}
        {...props}
      />
    );
  },
);
TabsList.displayName = TabsPrimitive.List.displayName;

interface TabsTriggerProps
  extends React.ComponentPropsWithoutRef<typeof TabsPrimitive.Trigger> {
  variant?: "default" | "pills" | "underlined" | "boxed" | "gradient";
  size?: "sm" | "md" | "lg";
  icon?: React.ReactNode;
}

const TabsTrigger = React.forwardRef<
  React.ElementRef<typeof TabsPrimitive.Trigger>,
  TabsTriggerProps & { isActive?: boolean }
>(
  (
    {
      className,
      variant = "default",
      size = "md",
      icon,
      children,
      isActive,
      ...props
    },
    ref,
  ) => {
    const variantStyles = {
      default: {
        base: "rounded-sm transition-all data-[state=active]:bg-background data-[state=active]:text-foreground data-[state=active]:shadow-sm",
        active: "",
      },
      pills: {
        base: "rounded-full transition-all border border-transparent",
        active:
          "data-[state=active]:bg-primary data-[state=active]:text-primary-foreground data-[state=active]:shadow-sm",
      },
      underlined: {
        base: "rounded-none border-b-2 border-transparent px-1 pb-3 pt-2 font-medium transition-all",
        active:
          "data-[state=active]:border-primary data-[state=active]:text-foreground",
      },
      boxed: {
        base: "rounded-md border border-transparent transition-all",
        active:
          "data-[state=active]:bg-background data-[state=active]:text-foreground data-[state=active]:border-border data-[state=active]:shadow-sm",
      },
      gradient: {
        base: "rounded-md transition-all overflow-hidden",
        active:
          "data-[state=active]:bg-gradient-to-r data-[state=active]:from-primary/80 data-[state=active]:to-primary data-[state=active]:text-primary-foreground data-[state=active]:shadow-md",
      },
    };

    const sizeStyles = {
      sm: "text-xs px-2.5 py-1",
      md: "text-sm px-3 py-1.5",
      lg: "text-base px-4 py-2",
    };

    // Use the data-state attribute from props if available, fallback to isActive prop
    const isTriggerActive =
      (props as any)["data-state"] === "active" || isActive;

    return (
      <TabsPrimitive.Trigger
        ref={ref}
        className={cn(
          "relative inline-flex items-center justify-center whitespace-nowrap font-medium ring-offset-background transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50",
          "hover:bg-muted/50 hover:text-foreground/80",
          variantStyles[variant || "default"].base,
          variantStyles[variant || "default"].active,
          sizeStyles[size || "md"],
          className,
        )}
        {...props}
      >
        {icon && <span className="mr-2">{icon}</span>}
        {children}
        {variant === "gradient" && isTriggerActive && (
          <motion.span
            layoutId={`tab-gradient-${props.value}`}
            className="absolute inset-0 -z-10 bg-gradient-to-r from-primary/80 to-primary"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.2 }}
          />
        )}
      </TabsPrimitive.Trigger>
    );
  },
);
TabsTrigger.displayName = TabsPrimitive.Trigger.displayName;

const TabsContent = React.forwardRef<
  React.ElementRef<typeof TabsPrimitive.Content>,
  React.ComponentPropsWithoutRef<typeof TabsPrimitive.Content>
>(({ className, ...props }, ref) => (
  <TabsPrimitive.Content
    ref={ref}
    className={cn(
      "mt-2 ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2",
      className,
    )}
    {...props}
  />
));
TabsContent.displayName = TabsPrimitive.Content.displayName;

export { Tabs, TabsList, TabsTrigger, TabsContent };
