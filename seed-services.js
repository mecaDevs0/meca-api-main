// Script para criar serviços base MECA
const fetch = require('node-fetch');

const services = [
  {
    name: "Troca de Óleo",
    description: "Troca de óleo do motor e filtro",
    category: "Manutenção Preventiva",
    estimated_duration_minutes: 30,
    suggested_price_min: 10000, // R$ 100
    suggested_price_max: 25000, // R$ 250
    icon_url: "https://meca-images.s3.amazonaws.com/icons/oil-change.png",
    display_order: 1
  },
  {
    name: "Alinhamento e Balanceamento",
    description: "Alinhamento e balanceamento de rodas",
    category: "Manutenção Preventiva",
    estimated_duration_minutes: 60,
    suggested_price_min: 15000,
    suggested_price_max: 30000,
    icon_url: "https://meca-images.s3.amazonaws.com/icons/alignment.png",
    display_order: 2
  },
  {
    name: "Troca de Pneus",
    description: "Troca de pneus com montagem e balanceamento",
    category: "Pneus e Rodas",
    estimated_duration_minutes: 90,
    suggested_price_min: 50000,
    suggested_price_max: 200000,
    icon_url: "https://meca-images.s3.amazonaws.com/icons/tires.png",
    display_order: 3
  },
  {
    name: "Revisão Geral",
    description: "Revisão completa do veículo conforme manual",
    category: "Manutenção Preventiva",
    estimated_duration_minutes: 120,
    suggested_price_min: 30000,
    suggested_price_max: 80000,
    icon_url: "https://meca-images.s3.amazonaws.com/icons/inspection.png",
    display_order: 4
  },
  {
    name: "Freios",
    description: "Troca de pastilhas e discos de freio",
    category: "Sistema de Freios",
    estimated_duration_minutes: 90,
    suggested_price_min: 40000,
    suggested_price_max: 150000,
    icon_url: "https://meca-images.s3.amazonaws.com/icons/brakes.png",
    display_order: 5
  }
];

async function seedServices() {
  console.log('🌱 Criando serviços base MECA...\n');
  
  for (const service of services) {
    try {
      const response = await fetch('http://localhost:9000/admin/master-services', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(service)
      });
      
      const data = await response.json();
      
      if (response.ok) {
        console.log(`✅ ${service.name} criado`);
      } else {
        console.log(`❌ ${service.name} - ${data.message}`);
      }
    } catch (error) {
      console.log(`❌ ${service.name} - Erro: ${error.message}`);
    }
  }
  
  console.log('\n✅ Seed completo!');
}

seedServices();

