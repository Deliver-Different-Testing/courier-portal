import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { resolveTenant } from '@/lib/tenants';

interface Profile {
  firstName: string; surname: string;
  phone: string; mobile: string; email: string;
  companyName: string; addressLine2: string; streetNumber: string; streetName: string;
  city: string; state: string; zipCode: string; country: string;
  driversLicenceNo: string; vehicleRegistrationNo: string;
  bankRoutingNumber: string; bankAccountNo: string; taxNo: string;
}

const MOCK_PROFILE: Profile = {
  firstName: 'Test', surname: 'Courier',
  phone: '09 555 1234', mobile: '021 555 6789', email: 'test@courier.co.nz',
  companyName: 'Test Courier Ltd', addressLine2: '', streetNumber: '42', streetName: 'Queen Street',
  city: 'Auckland', state: 'Auckland', zipCode: '1010', country: 'New Zealand',
  driversLicenceNo: 'AB123456', vehicleRegistrationNo: 'ABC123',
  bankRoutingNumber: '', bankAccountNo: '06-0123-0456789-00', taxNo: '123-456-789',
};

export default function CourierSettings() {
  const { tenantSlug } = useParams<{ tenantSlug: string }>();
  const tenant = resolveTenant(tenantSlug);
  const isNZ = tenant.country === 'NZ';

  const [section, setSection] = useState<'menu' | 'profile' | 'password'>('menu');
  const [profile, setProfile] = useState(MOCK_PROFILE);
  const [password, setPassword] = useState({ oldPassword: '', newPassword: '', confirmPassword: '' });
  const [saving, setSaving] = useState(false);

  const update = (field: keyof Profile, value: string) => setProfile(p => ({ ...p, [field]: value }));

  const saveProfile = () => {
    setSaving(true);
    setTimeout(() => { setSaving(false); alert('Profile saved'); }, 500);
  };

  const savePassword = () => {
    if (!password.oldPassword || !password.newPassword) { alert('All fields required'); return; }
    if (password.newPassword !== password.confirmPassword) { alert('Passwords do not match'); return; }
    setSaving(true);
    setTimeout(() => { setSaving(false); setSection('menu'); alert('Password changed'); }, 500);
  };

  const Field = ({ label, field, type = 'text', placeholder }: {
    label: string; field: keyof Profile; type?: string; placeholder?: string;
  }) => (
    <div>
      <label className="block text-xs font-medium text-gray-500 mb-1">{label}</label>
      <input type={type} value={profile[field]}
        onChange={e => update(field, e.target.value)}
        placeholder={placeholder || `Enter ${label.toLowerCase()}…`}
        className="w-full h-11 px-3 rounded-xl border border-gray-300 text-sm focus:ring-2 focus:border-transparent"
        style={{ '--tw-ring-color': tenant.accentColor } as React.CSSProperties} />
    </div>
  );

  // Menu
  if (section === 'menu') {
    const items = [
      { key: 'profile' as const, label: 'My Profile', icon: '👤' },
      { key: 'password' as const, label: 'Change Password', icon: '🔑' },
    ];
    return (
      <div className="px-4 pt-4">
        <h1 className="text-xl font-bold text-gray-900 mb-4">Settings</h1>
        {items.map(item => (
          <button key={item.key} onClick={() => setSection(item.key)}
            className="w-full flex items-center gap-3 bg-white rounded-xl p-4 shadow-sm border border-gray-100 mb-3 text-left active:bg-gray-50">
            <span className="text-xl">{item.icon}</span>
            <span className="font-medium text-gray-900 text-sm">{item.label}</span>
            <span className="ml-auto text-gray-400">›</span>
          </button>
        ))}
      </div>
    );
  }

  // Profile
  if (section === 'profile') {
    return (
      <div className="px-4 pt-4 pb-8">
        <button onClick={() => setSection('menu')} className="flex items-center text-sm font-medium mb-4"
          style={{ color: tenant.accentColor }}>← Back</button>

        <div className="flex items-center gap-3 mb-6">
          <div className="w-14 h-14 rounded-full flex items-center justify-center text-xl font-bold text-white"
               style={{ background: tenant.accentColor }}>
            {profile.firstName.charAt(0)}{profile.surname.charAt(0)}
          </div>
          <div>
            <div className="font-bold text-gray-900">{profile.firstName} {profile.surname}</div>
            <div className="text-sm text-gray-500">{profile.email}</div>
          </div>
        </div>

        <div className="space-y-3">
          <Field label="First Name" field="firstName" />
          <Field label="Surname" field="surname" />
          <Field label="Phone" field="phone" type="tel" />
          <Field label="Mobile" field="mobile" type="tel" />
          <Field label="Email" field="email" type="email" />

          <div className="border-t border-gray-200 my-4" />
          <h3 className="text-sm font-semibold text-gray-700">Address</h3>
          <Field label="Company Name" field="companyName" />
          <Field label="Apt / Floor / Unit" field="addressLine2" />
          <Field label="Street Number" field="streetNumber" />
          <Field label="Street Name" field="streetName" />
          <Field label="City" field="city" />
          <Field label="State" field="state" />
          <Field label="Zip Code" field="zipCode" />
          <Field label="Country" field="country" />

          <div className="border-t border-gray-200 my-4" />
          <h3 className="text-sm font-semibold text-gray-700">Vehicle & Licence</h3>
          <Field label="Driver's Licence No" field="driversLicenceNo" />
          <Field label="Vehicle Registration No" field="vehicleRegistrationNo" />

          <div className="border-t border-gray-200 my-4" />
          <h3 className="text-sm font-semibold text-gray-700">Bank Details</h3>
          {!isNZ && <Field label="Bank Routing Number" field="bankRoutingNumber" />}
          <Field label="Bank Account No" field="bankAccountNo" />
          <Field label={isNZ ? 'IRD / GST No' : 'SSN'} field="taxNo" />
        </div>

        <button onClick={saveProfile} disabled={saving}
          className="w-full mt-6 py-3 rounded-xl text-sm font-semibold text-white disabled:opacity-50 active:scale-[0.97]"
          style={{ background: tenant.accentColor }}>
          {saving ? 'Saving…' : 'Save Profile'}
        </button>
      </div>
    );
  }

  // Password
  return (
    <div className="px-4 pt-4">
      <button onClick={() => setSection('menu')} className="flex items-center text-sm font-medium mb-4"
        style={{ color: tenant.accentColor }}>← Back</button>

      <h2 className="text-lg font-bold text-gray-900 mb-2">Change Password</h2>
      <div className="bg-amber-50 text-amber-700 text-xs px-4 py-3 rounded-xl mb-4">
        ⚠️ This will change your password for the Courier Portal and the mobile app.
      </div>

      <div className="space-y-3">
        {([
          ['Old Password', 'oldPassword'],
          ['New Password', 'newPassword'],
          ['Confirm Password', 'confirmPassword'],
        ] as const).map(([label, field]) => (
          <div key={field}>
            <label className="block text-xs font-medium text-gray-500 mb-1">{label}</label>
            <input type="password" value={password[field]}
              onChange={e => setPassword(p => ({ ...p, [field]: e.target.value }))}
              placeholder={`Enter ${label.toLowerCase()}…`}
              className="w-full h-11 px-3 rounded-xl border border-gray-300 text-sm" />
          </div>
        ))}
      </div>

      <button onClick={savePassword} disabled={saving}
        className="w-full mt-6 py-3 rounded-xl text-sm font-semibold text-white disabled:opacity-50 active:scale-[0.97]"
        style={{ background: tenant.accentColor }}>
        {saving ? 'Saving…' : 'Save Password'}
      </button>
    </div>
  );
}
