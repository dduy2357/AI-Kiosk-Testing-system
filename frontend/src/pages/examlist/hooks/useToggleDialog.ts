import { useState } from "react";

const useToggleDialog = () => {
  const [isOpen, setIsOpen] = useState(false);

  const toggle = () => {
    setIsOpen((prev) => !prev);
  };

  const open = () => {
    setIsOpen(true);
  };

  const close = () => {
    setIsOpen(false);
  };

  return [isOpen, toggle, isOpen, open, close] as const;
};

export default useToggleDialog;
