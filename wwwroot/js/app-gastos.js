// GET - Consome os dados da API REST e popula o HTML
async function carregarGastos() {
    try {
        const resposta = await fetch(`/api/gastosapi/${USUARIO_ID_TESTE}`);
        const gastos = await resposta.json();
        
        console.log("Gastos recebidos da API:", gastos); // Para verificarmos no F12
        
        const corpoTabela = document.getElementById("tabela-corpo-gastos");
        if (!corpoTabela) return; // Proteção caso o ID não exista na tela

        corpoTabela.innerHTML = ""; // Limpa a tabela antiga
        
        if (gastos.length === 0) {
            corpoTabela.innerHTML = `
                <tr>
                    <td colspan="5" class="text-center text-muted py-4">
                        <i class="bi bi-info-circle me-2"></i>Nenhum gasto lançado este mês.
                    </td>
                </tr>`;
            return;
        }

        gastos.forEach(item => {
            // Recria o visual das tuas categorias (Fixo/Variável)
            const badgeClass = item.categoria === "Fixo" ? "bg-primary" : "bg-warning text-dark";
            const iconeFixo = item.categoria === "Fixo" ? `<i class="bi bi-pin-angle-fill me-1"></i>` : "";
            
            // Injeta a nova linha
            corpoTabela.innerHTML += `
                <tr>
                    <td>${item.descricao}</td>
                    <td>
                        <span class="badge ${badgeClass}">
                            ${iconeFixo}${item.categoria}
                        </span>
                    </td>
                    <td>${item.dataGasto}</td>
                    <td class="fw-bold">R$ ${Number(item.valor).toFixed(2)}</td>
                    <td>
                        <i class="bi bi-pencil-square text-muted cursor-pointer btn-editar"
                           data-id="${item.id}"
                           data-descricao="${item.descricao}"
                           data-categoria="${item.categoria}"
                           data-valor="${item.valor}">
                        </i>
                    </td>
                </tr>
            `;
        });
        
    } catch (erro) {
        console.error("Erro ao carregar a API de gastos:", erro);
    }
}