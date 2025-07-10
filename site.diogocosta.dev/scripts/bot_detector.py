#!/usr/bin/env python3
"""
Sistema Robô Detector de Spam para site.diogocosta.dev
Este script monitora logs e detecta novos padrões de bots automaticamente.

Uso:
    python bot_detector.py --config config.json
    python bot_detector.py --log-file /path/to/app.log --api-url https://site.com/api/antispam

Autor: Sistema Anti-Spam Dinâmico
Data: 2025-07-10
"""

import re
import json
import time
import requests
import argparse
from datetime import datetime, timedelta
from collections import defaultdict, Counter
from typing import Dict, List, Set
import logging

# Configurar logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('bot_detector.log'),
        logging.StreamHandler()
    ]
)
logger = logging.getLogger(__name__)

class BotDetector:
    def __init__(self, api_url: str, api_key: str = None):
        self.api_url = api_url.rstrip('/')
        self.api_key = api_key
        self.detected_patterns = set()
        self.ip_attempts = defaultdict(int)
        self.user_agent_attempts = defaultdict(int)
        self.email_domains = defaultdict(int)
        self.suspicious_names = defaultdict(int)
        
        # Padrões conhecidos de bots russos
        self.russian_patterns = [
            r'Поздравляем',
            r'Wilberries',
            r'выбраны для участия',
            r'бесплатные попытки',
            r'Поздравялем'
        ]
        
        # IPs suspeitos conhecidos
        self.suspicious_ips = {
            "45.141.215.111", "103.251.167.20", "192.42.116.217",
            "154.41.95.2", "185.246.188.74", "45.90.185.110"
        }
        
        # User-Agents suspeitos
        self.suspicious_user_agents = [
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36"
        ]

    def analyze_log_line(self, line: str) -> Dict:
        """Analisa uma linha de log e extrai informações relevantes"""
        result = {
            'timestamp': None,
            'ip': None,
            'user_agent': None,
            'email': None,
            'name': None,
            'is_suspicious': False,
            'reasons': []
        }
        
        # Extrair timestamp
        timestamp_match = re.search(r'\[([\d\-\s:]+)\]', line)
        if timestamp_match:
            result['timestamp'] = timestamp_match.group(1)
        
        # Extrair IP
        ip_match = re.search(r'IP:\s*([0-9\.]+)', line)
        if ip_match:
            result['ip'] = ip_match.group(1)
        
        # Extrair User-Agent
        ua_match = re.search(r'UserAgent:\s*([^\s,]+(?:\s+[^\s,]+)*)', line)
        if ua_match:
            result['user_agent'] = ua_match.group(1)
        
        # Extrair Email
        email_match = re.search(r'Email:\s*([^\s,]+@[^\s,]+)', line)
        if email_match:
            result['email'] = email_match.group(1)
        
        # Extrair Nome
        name_match = re.search(r'Nome:\s*([^,\s]+(?:\s+[^,\s]+)*)', line)
        if name_match:
            result['name'] = name_match.group(1)
        
        # Verificar se é suspeito
        self._check_suspicious(result)
        
        return result

    def _check_suspicious(self, data: Dict):
        """Verifica se os dados são suspeitos"""
        reasons = []
        
        # Verificar IP
        if data['ip']:
            if data['ip'] in self.suspicious_ips:
                reasons.append(f"IP conhecido: {data['ip']}")
            self.ip_attempts[data['ip']] += 1
            if self.ip_attempts[data['ip']] > 5:  # Mais de 5 tentativas
                reasons.append(f"IP com muitas tentativas: {data['ip']}")
        
        # Verificar User-Agent
        if data['user_agent']:
            self.user_agent_attempts[data['user_agent']] += 1
            if data['user_agent'] in self.suspicious_user_agents:
                reasons.append(f"User-Agent suspeito: {data['user_agent']}")
        
        # Verificar email
        if data['email']:
            domain = data['email'].split('@')[-1].lower()
            self.email_domains[domain] += 1
            
            # Domínios temporários conhecidos
            temp_domains = ['tempmail.com', '10minutemail.com', 'guerrillamail.com']
            if domain in temp_domains:
                reasons.append(f"Email temporário: {domain}")
        
        # Verificar nome com padrões russos
        if data['name']:
            self.suspicious_names[data['name']] += 1
            for pattern in self.russian_patterns:
                if re.search(pattern, data['name'], re.IGNORECASE):
                    reasons.append(f"Padrão russo no nome: {pattern}")
        
        data['is_suspicious'] = len(reasons) > 0
        data['reasons'] = reasons

    def add_rule_to_api(self, rule_type: str, rule_value: str, description: str, severity: str = "medium") -> bool:
        """Adiciona uma regra via API"""
        try:
            url = f"{self.api_url}/detect-and-add"
            
            payload = {
                "description": description,
                "severity": severity
            }
            
            if rule_type == "ip":
                payload["ipAddress"] = rule_value
            elif rule_type == "user_agent":
                payload["userAgent"] = rule_value
            elif rule_type == "domain":
                payload["emailDomain"] = rule_value
            elif rule_type == "name_pattern":
                payload["namePattern"] = rule_value
                payload["isRegex"] = True
            
            headers = {"Content-Type": "application/json"}
            if self.api_key:
                headers["Authorization"] = f"Bearer {self.api_key}"
            
            response = requests.post(url, json=payload, headers=headers, timeout=10)
            
            if response.status_code == 200:
                logger.info(f"✅ Regra adicionada com sucesso: {rule_type} - {rule_value}")
                return True
            else:
                logger.error(f"❌ Erro ao adicionar regra: {response.status_code} - {response.text}")
                return False
                
        except Exception as e:
            logger.error(f"❌ Erro na requisição API: {e}")
            return False

    def analyze_patterns_and_add_rules(self):
        """Analisa padrões detectados e adiciona regras automaticamente"""
        logger.info("🔍 Analisando padrões detectados...")
        
        # IPs com muitas tentativas
        for ip, count in self.ip_attempts.items():
            if count >= 10 and ip not in self.detected_patterns:  # Threshold de 10 tentativas
                self.add_rule_to_api(
                    "ip", 
                    ip, 
                    f"IP com {count} tentativas suspeitas detectadas automaticamente",
                    "high"
                )
                self.detected_patterns.add(ip)
        
        # User-Agents repetitivos
        for ua, count in self.user_agent_attempts.items():
            if count >= 15 and ua not in self.detected_patterns:  # Threshold de 15 tentativas
                self.add_rule_to_api(
                    "user_agent",
                    ua,
                    f"User-Agent com {count} tentativas suspeitas detectadas automaticamente",
                    "medium"
                )
                self.detected_patterns.add(ua)
        
        # Domínios de email suspeitos
        for domain, count in self.email_domains.items():
            if count >= 5 and domain not in self.detected_patterns:  # Threshold de 5 tentativas
                # Verificar se é domínio suspeito (.ru, .tk, etc)
                suspicious_tlds = ['.ru', '.tk', '.ml', '.ga']
                if any(domain.endswith(tld) for tld in suspicious_tlds):
                    self.add_rule_to_api(
                        "domain",
                        domain,
                        f"Domínio suspeito com {count} tentativas detectadas automaticamente",
                        "high"
                    )
                    self.detected_patterns.add(domain)
        
        # Nomes suspeitos repetitivos
        for name, count in self.suspicious_names.items():
            if count >= 3 and name not in self.detected_patterns:  # Threshold de 3 tentativas
                # Verificar se contém caracteres não-latinos
                if re.search(r'[а-яё]', name, re.IGNORECASE):  # Cirílico
                    self.add_rule_to_api(
                        "name_pattern",
                        re.escape(name),  # Escape para regex
                        f"Nome com caracteres cirílicos detectado {count} vezes automaticamente",
                        "critical"
                    )
                    self.detected_patterns.add(name)

    def monitor_log_file(self, log_file_path: str, follow: bool = True):
        """Monitora arquivo de log em tempo real"""
        logger.info(f"🔍 Iniciando monitoramento do arquivo: {log_file_path}")
        
        try:
            with open(log_file_path, 'r', encoding='utf-8') as f:
                # Ir para o final do arquivo se follow=True
                if follow:
                    f.seek(0, 2)
                
                last_analysis = datetime.now()
                
                while True:
                    line = f.readline()
                    
                    if line:
                        # Processar linha
                        if 'Email:' in line or 'Nome:' in line or 'IP:' in line:
                            data = self.analyze_log_line(line.strip())
                            
                            if data['is_suspicious']:
                                logger.warning(f"⚠️  Atividade suspeita detectada: {data['reasons']}")
                                logger.info(f"📊 Dados: IP={data['ip']}, Email={data['email']}, Nome={data['name']}")
                    
                    else:
                        if not follow:
                            break
                        
                        # Verificar se deve analisar padrões (a cada 5 minutos)
                        if datetime.now() - last_analysis > timedelta(minutes=5):
                            self.analyze_patterns_and_add_rules()
                            last_analysis = datetime.now()
                        
                        time.sleep(1)  # Aguardar novas linhas
                        
        except FileNotFoundError:
            logger.error(f"❌ Arquivo de log não encontrado: {log_file_path}")
        except KeyboardInterrupt:
            logger.info("🛑 Monitoramento interrompido pelo usuário")
        except Exception as e:
            logger.error(f"❌ Erro durante monitoramento: {e}")

    def get_stats(self) -> Dict:
        """Retorna estatísticas das detecções"""
        return {
            "ip_attempts": dict(self.ip_attempts),
            "user_agent_attempts": dict(self.user_agent_attempts),
            "email_domains": dict(self.email_domains),
            "suspicious_names": dict(self.suspicious_names),
            "total_patterns_detected": len(self.detected_patterns),
            "detected_patterns": list(self.detected_patterns)
        }

def main():
    parser = argparse.ArgumentParser(description='Bot Detector para Sistema Anti-Spam')
    parser.add_argument('--log-file', required=True, help='Caminho para o arquivo de log')
    parser.add_argument('--api-url', required=True, help='URL da API de anti-spam')
    parser.add_argument('--api-key', help='Chave da API (opcional)')
    parser.add_argument('--follow', action='store_true', help='Monitora o arquivo em tempo real')
    parser.add_argument('--config', help='Arquivo de configuração JSON')
    
    args = parser.parse_args()
    
    # Carregar configuração se fornecida
    if args.config:
        try:
            with open(args.config, 'r') as f:
                config = json.load(f)
                args.log_file = config.get('log_file', args.log_file)
                args.api_url = config.get('api_url', args.api_url)
                args.api_key = config.get('api_key', args.api_key)
        except Exception as e:
            logger.error(f"❌ Erro ao carregar configuração: {e}")
            return
    
    # Inicializar detector
    detector = BotDetector(args.api_url, args.api_key)
    
    logger.info("🤖 Bot Detector iniciado!")
    logger.info(f"📂 Arquivo de log: {args.log_file}")
    logger.info(f"🔗 API URL: {args.api_url}")
    logger.info(f"🔄 Modo follow: {args.follow}")
    
    # Monitorar arquivo
    detector.monitor_log_file(args.log_file, args.follow)
    
    # Mostrar estatísticas finais
    stats = detector.get_stats()
    logger.info("📊 Estatísticas finais:")
    logger.info(f"   IPs monitorados: {len(stats['ip_attempts'])}")
    logger.info(f"   User-Agents monitorados: {len(stats['user_agent_attempts'])}")
    logger.info(f"   Domínios de email: {len(stats['email_domains'])}")
    logger.info(f"   Nomes suspeitos: {len(stats['suspicious_names'])}")
    logger.info(f"   Padrões detectados e adicionados: {stats['total_patterns_detected']}")

if __name__ == "__main__":
    main()
