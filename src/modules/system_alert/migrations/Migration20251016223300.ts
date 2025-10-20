import { Migration } from '@mikro-orm/migrations';

export class Migration20251016223300 extends Migration {

  override async up(): Promise<void> {
    this.addSql(`create table if not exists "system_alert" ("id" text not null, "type" text check ("type" in ('anomaly', 'warning', 'error', 'info', 'security')) not null, "severity" text check ("severity" in ('low', 'medium', 'high', 'critical')) not null, "category" text check ("category" in ('workshop', 'client', 'booking', 'payment', 'review', 'system')) not null, "title" text not null, "message" text not null, "details" jsonb null, "entity_type" text null, "entity_id" text null, "action_required" boolean not null default false, "action_url" text null, "is_read" boolean not null default false, "is_resolved" boolean not null default false, "resolved_at" timestamptz null, "resolved_by" text null, "auto_generated" boolean not null default true, "triggered_by" text null, "created_at" timestamptz not null default now(), "updated_at" timestamptz not null default now(), "deleted_at" timestamptz null, constraint "system_alert_pkey" primary key ("id"));`);
    this.addSql(`CREATE INDEX IF NOT EXISTS "IDX_system_alert_deleted_at" ON "system_alert" (deleted_at) WHERE deleted_at IS NULL;`);
  }

  override async down(): Promise<void> {
    this.addSql(`drop table if exists "system_alert" cascade;`);
  }

}
