import { ROLE_ENUM } from '@/consts/role';
import AccessDenied from '@/pages/AccessDenied';
import { useAuth } from '@/providers/AuthenticationProvider';
import React, { ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';

interface WithCheckRoleProps {
  children?: ReactNode;
  [key: string]: any;
}

const withCheckRole = <P extends object>(
  WrappedComponent: React.ComponentType<P>,
  permissions?: number[],
) => {
  const WithPermission = (props: P & WithCheckRoleProps) => {
    const navigate = useNavigate();
    const { user } = useAuth();
    const role = user?.roleId ?? [ROLE_ENUM.Student];

    const havePermission = permissions ? permissions.some((perm) => role.includes(perm)) : true;

    if (havePermission) {
      return <WrappedComponent {...props} />;
    }

    return (
      <AccessDenied
        onBack={() => {
          navigate(-1);
        }}
      />
    );
  };

  return React.memo(WithPermission);
};

export default withCheckRole;
