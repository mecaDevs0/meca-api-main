/**
 * Script para popular banco PostgreSQL com dados do backup MongoDB
 * Execução: node populate-from-backup.js
 */

const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

const BACKUP_DIR = '/Users/filippoferrari/Documents/ARQUIVOS-MECA/Meca';

// Extrair dados do MongoDB BSON
function extractCollection(collectionName) {
  const bsonFile = path.join(BACKUP_DIR, `${collectionName}.bson.gz`);
  const cmd = `gunzip -c "${bsonFile}" | bsondump --type=json`;
  
  try {
    const output = execSync(cmd, { encoding: 'utf-8', maxBuffer: 50 * 1024 * 1024 });
    return output.split('\n')
      .filter(line => line.trim())
      .map(line => {
        try {
          return JSON.parse(line);
        } catch {
          return null;
        }
      })
      .filter(item => item !== null);
  } catch (error) {
    console.error(`Erro ao extrair ${collectionName}:`, error.message);
    return [];
  }
}

// Converter Workshop do MongoDB para Oficina do Medusa
function convertWorkshopToOficina(workshop) {
  return {
    name: workshop.CompanyName,
    cnpj: workshop.Cnpj,
    email: workshop.Email,
    phone: workshop.Phone,
    address: {
      cep: workshop.ZipCode,
      rua: workshop.StreetAddress,
      numero: workshop.Number,
      complemento: workshop.Complement,
      bairro: workshop.Neighborhood,
      cidade: workshop.CityName,
      estado: workshop.StateUf,
      latitude: workshop.Latitude?.$numberDouble,
      longitude: workshop.Longitude?.$numberDouble,
    },
    owner_name: workshop.FullName,
    owner_cpf: workshop.Cpf,
    birth_date: workshop.BirthDate,
    bank_details: workshop.HasDataBank ? {
      banco: workshop.Bank,
      banco_nome: workshop.BankName,
      agencia: workshop.BankAgency,
      conta: workshop.BankAccount,
      titular: workshop.AccountableName,
      titular_cpf: workshop.AccountableCpf,
    } : null,
    status: workshop.Status?.$numberInt === 1 ? 'aprovado' : 'pendente',
    photo_url: workshop.Photo,
  };
}

// Mapeamento de serviços para categorias
function categorizeService(serviceName) {
  const name = serviceName.toLowerCase();
  
  if (name.includes('óleo') || name.includes('filtro')) return 'troca_oleo';
  if (name.includes('freio')) return 'freios';
  if (name.includes('suspensão') || name.includes('amortecedor')) return 'suspensao';
  if (name.includes('motor') || name.includes('câmbio')) return 'motor';
  if (name.includes('elétrica') || name.includes('bateria') || name.includes('diagnóstico')) return 'eletrica';
  if (name.includes('ar-condicionado') || name.includes('climatização')) return 'ar_condicionado';
  if (name.includes('alinhamento') || name.includes('balanceamento')) return 'alinhamento_balanceamento';
  if (name.includes('revisão') || name.includes('preventiva')) return 'manutencao_preventiva';
  
  return 'outros';
}

// Converter ServicesDefault para MasterService
function convertServiceToMasterService(service) {
  return {
    title: service.Name,
    description: service.Description,
    category: categorizeService(service.Name),
    min_time_hours: service.MinTimeScheduling?.$numberDouble || 1,
    photo_url: service.Photo,
  };
}

// Converter Vehicle
function convertVehicle(vehicle) {
  return {
    marca: vehicle.Manufacturer,
    modelo: vehicle.Model,
    ano: vehicle.Year,
    placa: vehicle.Plate,
    cor: vehicle.Color,
    km_atual: vehicle.Km?.$numberDouble,
    customer_email: vehicle.Profile?.Email,
    customer_name: vehicle.Profile?.FullName,
  };
}

async function main() {
  console.log('🔄 Extraindo dados do backup MongoDB...\n');
  
  // 1. Extrair Workshops
  console.log('📂 Extraindo Workshops...');
  const workshops = extractCollection('Workshop');
  console.log(`   ✔ ${workshops.length} oficinas encontradas`);
  workshops.forEach((w, i) => {
    console.log(`   ${i+1}. ${w.CompanyName} - ${w.CityName}/${w.StateUf}`);
  });
  
  // 2. Extrair Services
  console.log('\n📂 Extraindo ServicesDefault...');
  const services = extractCollection('ServicesDefault');
  console.log(`   ✔ ${services.length} serviços encontrados`);
  services.slice(0, 10).forEach((s, i) => {
    console.log(`   ${i+1}. ${s.Name}`);
  });
  if (services.length > 10) {
    console.log(`   ... e mais ${services.length - 10} serviços`);
  }
  
  // 3. Extrair Vehicles
  console.log('\n📂 Extraindo Vehicles...');
  const vehicles = extractCollection('Vehicle');
  console.log(`   ✔ ${vehicles.length} veículos encontrados`);
  vehicles.slice(0, 5).forEach((v, i) => {
    console.log(`   ${i+1}. ${v.Manufacturer} ${v.Model} - ${v.Plate}`);
  });
  
  // 4. Salvar em formato para popular no Medusa
  const data = {
    oficinas: workshops.map(convertWorkshopToOficina),
    services: services.map(convertServiceToMasterService),
    vehicles: vehicles.map(convertVehicle),
  };
  
  fs.writeFileSync(
    path.join(__dirname, 'backup-converted.json'),
    JSON.stringify(data, null, 2)
  );
  
  console.log('\n✅ Dados convertidos salvos em: backup-converted.json');
  console.log(`\n📊 Resumo:`);
  console.log(`   - ${data.oficinas.length} oficinas`);
  console.log(`   - ${data.services.length} serviços`);
  console.log(`   - ${data.vehicles.length} veículos`);
}

main().catch(console.error);



