import { useEffect } from 'react';

export const usePreventBackNavigation = (when = true) => {
  useEffect(() => {
    if (!when) return; // Do nothing if `when` is false

    // Function to disable back navigation
    const disableBack = () => {
      window.history.pushState(null, document.title, window.location.pathname);
    };

    // Add event listener to handle back button
    window.addEventListener('popstate', disableBack);

    // Initial push to prevent back navigation
    disableBack();

    // Cleanup event listener
    return () => {
      window.removeEventListener('popstate', disableBack);
    };
  }, [when]);
};