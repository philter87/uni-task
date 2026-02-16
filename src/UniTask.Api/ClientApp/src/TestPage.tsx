import { TestItem, testValue } from './api/simple-test';

export function TestPage() {
  const item: TestItem = { id: 1, name: 'Test' };
  return <div>Test: {item.name}, Value: {testValue}</div>;
}
