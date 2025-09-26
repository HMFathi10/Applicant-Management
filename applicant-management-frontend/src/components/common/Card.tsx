import React from 'react';
import type { ReactNode } from 'react';

export interface CardProps {
  children: ReactNode;
  className?: string;
  header?: ReactNode;
  footer?: ReactNode;
  title?: string;
  subtitle?: string;
}

export const Card: React.FC<CardProps> = ({ 
  children, 
  className = '', 
  header, 
  footer, 
  title, 
  subtitle 
}) => {
  const classes = ['card', className].filter(Boolean).join(' ');

  return (
    <div className={classes}>
      {(header || title || subtitle) && (
        <div className="card__header">
          {header || (
            <>
              {title && <h3 className="card__title">{title}</h3>}
              {subtitle && <p className="card__subtitle">{subtitle}</p>}
            </>
          )}
        </div>
      )}
      <div className="card__body">
        {children}
      </div>
      {footer && (
        <div className="card__footer">
          {footer}
        </div>
      )}
    </div>
  );
};

export default Card;