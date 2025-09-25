import { AdditionalFormikProps } from '@/interfaces/common';
import { useFormikContext } from 'formik';
import { get, isString } from 'lodash';
import React, { ChangeEvent, useCallback, useEffect, useState, useMemo } from 'react';
import { twMerge } from 'tailwind-merge';
import CommonIcons from '../commonIcons';
import { Input, InputProps } from '../ui/input';
import { Label } from '../ui/label';
import { NumericFormat } from 'react-number-format';

interface InputFieldProps extends InputProps {
  label?: string | React.ReactNode;
  nextText?: string | React.ReactNode;
  required?: boolean;
  classNameLabel?: string;
  classNameContainer?: string;
  afterOnChange?: (e: ChangeEvent) => void;
  isIcon?: boolean;
  iconText?: string;
  propValue?: number | string;
  onCustomChange?: (e: ChangeEvent) => void;
  isNumberic?: boolean;
  unitNumberic?: string;
}

const InputField = ({
  label,
  nextText,
  required,
  classNameLabel,
  classNameContainer,
  form,
  field,
  className,
  propValue = '',
  onCustomChange,
  afterOnChange,
  isNumberic,
  unitNumberic,
  extraRight: extraRightElm,
  extraLeft,
  disabled,
  type = 'text',
  placeholder,
  iconText,
  ...restPropsInput
}: InputFieldProps & AdditionalFormikProps) => {
  const { name, value, onBlur, onChange } = field;
  const { errors, touched, setTouched } = form;
  const { setFieldValue } = useFormikContext();

  const [showPassword, setShowPassword] = useState(false);

  const msgError = useMemo(
    () => get(touched, name) && (get(errors, name) as string),
    [touched, errors, name],
  );

  useEffect(() => {
    if (propValue !== '') {
      setFieldValue(name, propValue);
    }
  }, [propValue, setFieldValue, name]);

  useEffect(() => {
    if (value !== 0 && value !== '') {
      setTouched({ [name]: true });
    }
  }, [value, name, setTouched]);

  const onHandleChange = (e: ChangeEvent) => {
    onCustomChange?.(e) ?? onChange(e);
    afterOnChange?.(e);
  };

  const handleClickShowPassword = useCallback(() => {
    setShowPassword((prev) => !prev);
  }, []);

  const extraRight = useMemo(() => {
    if (type !== 'password') return extraRightElm;
    return showPassword ? (
      <CommonIcons.EyeOff onClick={handleClickShowPassword} />
    ) : (
      <CommonIcons.Eye onClick={handleClickShowPassword} />
    );
  }, [type, showPassword, extraRightElm, handleClickShowPassword]);

  const inputClass = twMerge(
    className,
    'typo-3 2xl:text-typo-2 placeholder-text-third rounded-[8px] px-[16px] py-[8px] !text-black focus:border-black focus-visible:ring-0 focus-visible:ring-offset-0 2xl:h-9 2xl:py-[12px]',
    disabled && 'bg-disabled !opacity-[0.8]',
    msgError && 'border-red-500 focus:border-red-500',
  );

  const inputType = type === 'password' ? (showPassword ? 'text' : 'password') : type;

  const renderInput = () => {
    if (isNumberic) {
      return (
        <NumericFormat
          onBlur={onBlur}
          value={value}
          placeholder={placeholder}
          onValueChange={({ value }) => {
            onChange({
              target: {
                name,
                value: value ?? '',
              },
            });
          }}
          allowLeadingZeros
          thousandSeparator="."
          decimalSeparator=","
          valueIsNumericString
          suffix={unitNumberic}
          disabled={disabled}
          onBeforeInput={(e: React.FormEvent<HTMLInputElement>) => {
            const inputEvent = e.nativeEvent as InputEvent;
            if (inputEvent.data && !/[0-9.,]/.test(inputEvent.data)) {
              e.preventDefault();
            }
          }}
          className={twMerge(inputClass, '!border-[1px] !border-[#e2e8f0] focus:border-black')}
        />
      );
    }

    return (
      <Input
        {...restPropsInput}
        name={name}
        id={name}
        type={inputType}
        value={value}
        onBlur={onBlur}
        onChange={onHandleChange}
        extraRight={extraRight}
        extraLeft={extraLeft}
        disabled={disabled}
        placeholder={placeholder}
        iconText={iconText}
        className={inputClass}
        onFocus={(e) => {
          if (type === 'number') {
            e.target.addEventListener('wheel', (evt) => evt.preventDefault(), {
              passive: false,
            });
          }
        }}
      />
    );
  };

  return (
    <div className={twMerge('grid w-full items-center gap-1.5', classNameContainer)}>
      {label && (
        <Label
          htmlFor={name}
          className={twMerge(
            'typo-7 mb-1 font-medium text-black',
            required && 'required',
            classNameLabel,
          )}
        >
          {label}
          {nextText && <span className="ml-1 text-gray-500">{nextText}</span>}
        </Label>
      )}
      {renderInput()}
      {isString(msgError) && <span className="invalid-text typo-3">{msgError}</span>}
    </div>
  );
};

export default InputField;
