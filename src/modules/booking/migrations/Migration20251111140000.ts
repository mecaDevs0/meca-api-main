import { Migration } from '@mikro-orm/migrations';

const NEW_STATUS_LIST = `'pending','confirmed','started','in_progress','completed','cancelled','suggested_time','pendente_oficina','pendente_cliente','confirmado','em_andamento','finalizado_aguardando_pagamento','pago','recusado','finalizado_mecanico','finalizado_cliente','cancelado','nao_compareceu'`;
const LEGACY_STATUS_LIST = `'pendente_oficina','confirmado','recusado','finalizado_mecanico','finalizado_cliente','cancelado','nao_compareceu'`;

export class Migration20251111140000 extends Migration {

  override async up(): Promise<void> {
    this.addSql(`alter table if exists "booking" drop constraint if exists "booking_status_check";`);
    this.addSql(`alter table if exists "booking" add constraint "booking_status_check" check (status in (${NEW_STATUS_LIST}));`);

    this.addSql(`alter table if exists "bookings" drop constraint if exists "booking_status_check";`);
    this.addSql(`alter table if exists "bookings" add constraint "booking_status_check" check (status in (${NEW_STATUS_LIST}));`);

    this.addSql(`alter table if exists "booking" add column if not exists "total_amount" numeric(12, 2);`);
    this.addSql(`alter table if exists "bookings" add column if not exists "total_amount" numeric(12, 2);`);
  }

  override async down(): Promise<void> {
    this.addSql(`alter table if exists "booking" drop constraint if exists "booking_status_check";`);
    this.addSql(`alter table if exists "booking" add constraint "booking_status_check" check (status in (${LEGACY_STATUS_LIST}));`);

    this.addSql(`alter table if exists "bookings" drop constraint if exists "booking_status_check";`);
    this.addSql(`alter table if exists "bookings" add constraint "booking_status_check" check (status in (${LEGACY_STATUS_LIST}));`);

    this.addSql(`alter table if exists "booking" drop column if exists "total_amount";`);
    this.addSql(`alter table if exists "bookings" drop column if exists "total_amount";`);
  }

}


