////// SCRIPT PADRÃO ///////
function formatarDocumento(campo) {
    // Remove tudo o que não é dígito
    let valor = campo.value.replace(/\D/g, "");

    // Limita a 14 caracteres (máximo de um CNPJ)
    if (valor.length > 14) {
        valor = valor.slice(0, 14);
    }

    // Aplica a máscara de CPF (11 dígitos) ou CNPJ (14 dígitos)
    if (valor.length <= 11) {
        // Máscara CPF: 000.000.000-00
        valor = valor.replace(/(\d{3})(\d)/, "$1.$2");
        valor = valor.replace(/(\d{3})(\d)/, "$1.$2");
        valor = valor.replace(/(\d{3})(\d{1,2})$/, "$1-$2");
    } else {
        // Máscara CNPJ: 00.000.000/0000-00
        valor = valor.replace(/^(\d{2})(\d)/, "$1.$2");
        valor = valor.replace(/^(\d{2})\.(\d{3})(\d)/, "$1.$2.$3");
        valor = valor.replace(/\.(\d{3})(\d)/, ".$1/$2");
        valor = valor.replace(/(\d{4})(\d)/, "$1-$2");
    }

    campo.value = valor;
}

function enviarFormulario() {
    var senha = document.getElementById('senha').value;
    var confirmarsenha = document.getElementById('confirmarsenha').value;

    if (senha !== confirmarsenha) {
        alert("Erro: As senhas precisam ser iguais para cadastrar.");
        event.preventDefault();
        return false;
    }

    return true
}

function formatarMoeda(input) {
    let valor = input.value.replace(/\D/g, ""); // Remove tudo que não é dígito
    valor = (valor / 100).toFixed(2) + "";
    valor = valor.replace(".", ",");
    valor = valor.replace(/(\d)(?=(\d{3})+(?!\d))/g, "$1.");
    input.value = "R$ " + valor;

}
function UpdateSalarioBruto(valor, id) {
    // Feedback visual de "carregando" (opcional)
    console.log("Atualizando id " + id + " para o valor: " + valor);

    fetch('/Home/UpdateSalarioBruto', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: `salario_bruto=${valor}&id=${id}`
    })
        .then(response => {
            if (response.ok) {
                // Se der certo, podemos dar um brilho verde no input
                console.log("Sucesso!");
            } else {
                console.error("Erro ao atualizar.");
            }
        })
        .catch(err => console.error("Erro de rede:", err));
}

function UpdateRegime(Valor, id) {
    fetch('/Home/UpdateRegime', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: `regime_trabalho=${Valor}&id=${id}`
    })
        .then(response => {
            if (response.ok) {
                // Se der certo, podemos dar um brilho verde no input
                console.log("Sucesso!");
            } else {
                console.error("Erro ao atualizar.");
            }
        })
        .catch(err => console.error("Erro de rede:", err));
}

function atualizarValores() {
    // 1. Busca o salário bruto
    let campoVisual = document.getElementById("inputSalarioVisual");
    if (!campoVisual) return;

    let brutoTexto = campoVisual.value.replace("R$", "").trim();
    if (brutoTexto.includes(",")) {
        brutoTexto = brutoTexto.replaceAll(".", "").replace(",", ".");
    }
    let bruto = parseFloat(brutoTexto) || 0;

    // 2. Percorre as linhas de desconto
    const rows = document.querySelectorAll('#containerDescontos .row');
    rows.forEach(row => {
        const inputPorcentagem = row.querySelector('.input-porcentagem');
        const inputValor = row.querySelector('.input-valor-calculado'); // A variável nasce aqui!

        if (inputPorcentagem && inputValor) {
            // Pega o valor da porcentagem e garante que o JS entenda a vírgula
            let textoPorc = inputPorcentagem.value.replace(',', '.');
            let porc = parseFloat(textoPorc) || 0;

            let calculado = bruto * (porc / 100);

            // Agora sim, o inputValor está dentro do bloco correto!
            if (bruto > 0) {
                inputValor.value = calculado.toFixed(2).replace('.', ',');
            } else {
                inputValor.value = "0,00";
            }
        }
    });
}

function sincronizarSalarioBruto(valorFormatado) {
    // 1. Remove o "R$" e espaços (se tiver)
    let valorLimpo = valorFormatado.replace('R$', '').trim();

    // 2. Remove os pontos (separadores de milhar)
    valorLimpo = valorLimpo.replaceAll('.', '');

    // 3. Troca a vírgula dos centavos por ponto (padrão que o C# e banco entendem)
    valorLimpo = valorLimpo.replace(',', '.');

    // 4. Salva no input escondido
    document.getElementById('salarioBrutoReal').value = valorLimpo;
}

// Chame a função também quando o salário bruto mudar
function formatarMoeda(input) {
    // ... seu código de formatação ...
    atualizarValores(); // Recalcula os descontos da lista
}


function carregarMercados() {
    fetch('/Home/ListarMercados')
        .then(res => res.json())
        .then(dados => {
            // Procure por todos os selects de mercado (pelo ID ou pela classe)
            // No seu HTML temos o id="selectMercado" no formulário e no modal
            const combos = document.querySelectorAll('select[id="selectMercado"]');

            combos.forEach(c => {
                if (c) { // Verifica se o elemento existe
                    c.innerHTML = '<option value="">Selecione o Mercado...</option>' +
                        dados.map(m => `<option value="${m.id}">${m.nome}</option>`).join('');
                }
            });
        })
        .catch(err => console.error("Erro ao carregar mercados:", err));
}

function atualizarLinha(linha) {
    let qtd = linha.querySelector('.qtd-item').value;
    let valor = linha.querySelector('.valor-unitario').value;

    let total = (qtd * valor) || 0;

    linha.querySelector('.total-item').value = total.toFixed(2);

    atualizarTotalGeral();
}

function atualizarTotalGeral() {
    let total = 0;

    document.querySelectorAll('.total-item').forEach(input => {
        total += parseFloat(input.value) || 0;
    });

    document.getElementById('totalGeralCompra').innerText =
        'R$ ' + total.toFixed(2);
}
function atualizarTotalGeral() {
    let total = 0;

    document.querySelectorAll('.total-item').forEach(input => {
        total += parseFloat(input.value) || 0;
    });

    document.getElementById('totalGeralCompra').innerText =
        'R$ ' + total.toFixed(2);
}

function carregarProdutos() {
    fetch('/Home/ListarProdutos')
        .then(res => res.json())
        .then(dados => {
            // Procure por todos os selects de produto pela classe .select-produto
            const combos = document.querySelectorAll('.select-produto, #selectProduto');

            combos.forEach(c => {
                if (c) { // Verifica se o elemento existe
                    c.innerHTML = '<option value="">Selecione o Produto...</option>' +
                        dados.map(p => `<option value="${p.id}">${p.nome}</option>`).join('');
                }
            });
        })
        .catch(err => console.error("Erro ao carregar produtos:", err));
}