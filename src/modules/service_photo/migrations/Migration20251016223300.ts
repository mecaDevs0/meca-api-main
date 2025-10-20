import { Migration } from '@mikro-orm/migrations';

export class Migration20251016223300 extends Migration {

  override async up(): Promise<void> {
    this.addSql(`create table if not exists "service_photo" ("id" text not null, "booking_id" text not null, "workshop_id" text not null, "photo_type" text check ("photo_type" in ('before', 'after', 'parts', 'problem', 'inspection')) not null, "url" text not null, "thumbnail_url" text null, "caption" text null, "taken_at" timestamptz not null, "uploaded_by" text not null, "file_size" integer null, "mime_type" text null, "width" integer null, "height" integer null, "created_at" timestamptz not null default now(), "updated_at" timestamptz not null default now(), "deleted_at" timestamptz null, constraint "service_photo_pkey" primary key ("id"));`);
    this.addSql(`CREATE INDEX IF NOT EXISTS "IDX_service_photo_deleted_at" ON "service_photo" (deleted_at) WHERE deleted_at IS NULL;`);
  }

  override async down(): Promise<void> {
    this.addSql(`drop table if exists "service_photo" cascade;`);
  }

}
