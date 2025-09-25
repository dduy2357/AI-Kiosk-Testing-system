import { ImageSource } from '@/assets';
import { DateTimeFormat } from '@/consts/dates';
import { RotateCw } from 'lucide-react';
import moment from 'moment';

import { Link } from 'react-router-dom';

export default function Footer() {
  const currentTime = moment().format(DateTimeFormat.Time);

  return (
    <footer className="bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900 text-white">
      {/* Main Footer Content */}
      <div className="container-fluid flex flex-row items-center justify-between px-2 py-2">
        {/* Company Info */}
        <div className="md:col-span-2">
          <div className="flex items-center space-x-3">
            <Link to="/" className="group flex items-center space-x-3">
              <div className="w-30 relative h-10 overflow-hidden rounded-lg bg-gradient-to-br from-blue-500 to-purple-600 transition-transform group-hover:scale-105">
                <img
                  loading="lazy"
                  src={ImageSource.LogoFPT ?? ''}
                  alt="Logo"
                  className="h-full w-full object-cover"
                />
              </div>
              <span className="bg-gradient-to-r from-blue-400 to-purple-400 bg-clip-text text-xl font-bold text-transparent">
                AKTs
              </span>
            </Link>
          </div>
        </div>

        {/* Quick Links */}
        <div className="flex flex-row items-center space-x-1">
          <span>{currentTime}</span>
          <RotateCw
            className="transition-colors duration-200 hover:cursor-pointer hover:text-blue-500"
            onClick={() => window.location.reload()}
          />
        </div>
      </div>
    </footer>
  );
}
