/**
 * SAAF - Sistema de Administração e Assistência Funerária
 * Funções JavaScript auxiliares
 */

/**
 * Baixa um arquivo em formato base64 para o navegador
 * Usado para download de PDFs, CSVs, etc.
 * 
 * @param {string} fileName - Nome do arquivo a baixar
 * @param {string} base64Content - Conteúdo em base64
 */
function downloadFile(fileName, base64Content) {
    try {
        // Cria um link de download
        const link = document.createElement('a');
        link.href = 'data:application/octet-stream;base64,' + base64Content;
        link.download = fileName;
        
        // Adiciona ao DOM, clica e remove
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        
        console.log(`Arquivo ${fileName} baixado com sucesso`);
    } catch (error) {
        console.error(`Erro ao baixar arquivo: ${error.message}`);
    }
}

/**
 * Exibe uma notificação no console com timestamp
 * 
 * @param {string} mensagem - Mensagem a exibir
 * @param {string} tipo - Tipo de log (log, warn, error, info)
 */
function logSakrus(mensagem, tipo = 'log') {
    const timestamp = new Date().toLocaleTimeString('pt-BR');
    const prefixo = `[SAAF ${timestamp}]`;
    console[tipo](`${prefixo} ${mensagem}`);
}

/**
 * Formata um valor monetário para R$ brasileiro
 * 
 * @param {number} valor - Valor em reais
 * @returns {string} Valor formatado (ex: R$ 1.234,56)
 */
function formatarMoeda(valor) {
    return new Intl.NumberFormat('pt-BR', {
        style: 'currency',
        currency: 'BRL'
    }).format(valor);
}

/**
 * Formata uma data para formato brasileiro
 * 
 * @param {Date|string} data - Data a formatar
 * @returns {string} Data formatada (dd/MM/yyyy)
 */
function formatarData(data) {
    if (typeof data === 'string') {
        data = new Date(data);
    }
    return new Intl.DateTimeFormat('pt-BR').format(data);
}

/**
 * Valida um CPF
 * 
 * @param {string} cpf - CPF a validar (com ou sem máscara)
 * @returns {boolean} True se válido
 */
function validarCPF(cpf) {
    cpf = cpf.replace(/\D/g, '');
    if (cpf.length !== 11 || /^(\d)\1{10}$/.test(cpf)) {
        return false;
    }
    
    let soma = 0;
    let resto;
    
    for (let i = 1; i <= 9; i++) {
        soma += parseInt(cpf.substring(i - 1, i)) * (11 - i);
    }
    
    resto = (soma * 10) % 11;
    if (resto === 10 || resto === 11) resto = 0;
    if (resto !== parseInt(cpf.substring(9, 10))) return false;
    
    soma = 0;
    for (let i = 1; i <= 10; i++) {
        soma += parseInt(cpf.substring(i - 1, i)) * (12 - i);
    }
    
    resto = (soma * 10) % 11;
    if (resto === 10 || resto === 11) resto = 0;
    if (resto !== parseInt(cpf.substring(10, 11))) return false;
    
    return true;
}

// Inicialização do SAAF
window.addEventListener('load', () => {
    logSakrus('Sistema SAAF iniciado com sucesso', 'info');
});
