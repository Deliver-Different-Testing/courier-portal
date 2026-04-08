import { useState } from 'react';
import { Outlet } from 'react-router-dom';
import Sidebar from './Sidebar';
import TopBar from './TopBar';
import { AutoMateAssistant } from '@/components/common/AutoMateAssistant';
import { CourierMessenger } from '@/components/common/CourierMessenger';

interface Props {
  onUpgrade?: () => void;
  selectedCourierId?: number | null;
}

export default function AppLayout({ onUpgrade, selectedCourierId }: Props) {
  const [collapsed, setCollapsed] = useState(false);
  const [messengerOpen, setMessengerOpen] = useState(false);

  return (
    <div className="flex min-h-screen bg-surface-light">
      <Sidebar collapsed={collapsed} onUpgrade={onUpgrade} selectedCourierId={selectedCourierId} />
      <div
        className={`flex-1 min-h-screen transition-[margin-left] duration-300 ${
          collapsed ? 'ml-0' : 'ml-64'
        }`}
      >
        <TopBar onToggle={() => setCollapsed(!collapsed)} onMessengerToggle={() => setMessengerOpen(!messengerOpen)} />
        <div className="p-6 fade-in">
          <Outlet />
        </div>
      </div>
      <AutoMateAssistant />
      <CourierMessenger open={messengerOpen} onClose={() => setMessengerOpen(false)} />
    </div>
  );
}
