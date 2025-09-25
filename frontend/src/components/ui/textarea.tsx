import * as React from "react";
import { cn } from "@/lib/utils";
import { Label } from "./label";
import { twMerge } from "tailwind-merge";
import { AdditionalFormikProps } from "@/interfaces/common";
import { get, isString } from "lodash";

export interface TextareaProps {
  label?: string | React.ReactNode;
  required?: boolean;
  className?: string;
  classNameLabel?: string;
  placeholder?: string;
}

const Textarea = (props: TextareaProps & AdditionalFormikProps) => {
  const {
    field,
    form,
    label,
    className,
    classNameLabel,
    required,
    placeholder,
  } = props;
  const { name, value, onBlur, onChange } = field;
  const { errors, touched, setTouched } = form;

  const msgError = React.useMemo(
    () => get(touched, name) && (get(errors, name) as string),
    [touched, errors, name],
  );

  React.useEffect(() => {
    if (value !== 0 && value !== "") {
      setTouched({ [name]: true });
    }
  }, [value, name, setTouched]);

  const inputClass = twMerge(
    className,
    "typo-3 2xl:text-typo-2 placeholder-text-third rounded-[8px] px-[16px] py-[8px] !text-black focus:border-black focus-visible:ring-0 focus-visible:ring-offset-0 2xl:h-9 2xl:py-[12px]",
    msgError && "border-red-500 focus:border-red-500",
  );

  return (
    <div>
      {label && (
        <Label
          className={twMerge("mb-1", required && "required", classNameLabel)}
        >
          {label}
        </Label>
      )}
      <textarea
        className={cn(
          "flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50",
          inputClass,
        )}
        placeholder={placeholder}
        id={name}
        name={name}
        value={value}
        onChange={onChange}
        onBlur={onBlur}
        required={required}
      />
      {isString(msgError) && (
        <span className="invalid-text typo-3">{msgError}</span>
      )}
    </div>
  );
};

export { Textarea };
