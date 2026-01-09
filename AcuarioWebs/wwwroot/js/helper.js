//Crear función que permita validar el correo
const validarMail = (email) => {
    //expresión regular
    const formato = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/
    //returna true o flase
    return formato.test(email)
}
//Crear función que permita sólo el ingreso de números
const soloNumeros = (evt) => {
    if (evt.keyCode >= 48 && evt.keyCode <= 57)
        return true
    return false
}
const limpiar = () => {
    document.querySelectorAll('form .form-control,.form-select').forEach(item => {
        item.value = ''
        item.classList.remove('is-invalid')
        item.classList.remove('is-valid')
        document.getElementById('e-' + item.id).innerHTML = ''
    })
}

const soloNumerosDecimal = (evt) => {
    const tecla = evt.key; // tecla presionada
    const input = evt.target; // input donde se escribe

    // Permitir números
    if (tecla >= '0' && tecla <= '9') {
        return true;
    }

    // Permitir un solo punto
    if (tecla === '.' && !input.value.includes('.')) {
        return true;
    }

    // Bloquear todo lo demás
    evt.preventDefault();
    return false;
};

const validaRun = (run) => {
    const Fn = {
        // Valida el rut con su cadena completa "XXXXXXXX-X"
        validaRut: function (rutCompleto) {
            rutCompleto = rutCompleto.replace("‐", "-");
            if (!/^[0-9]+[-|‐]{1}[0-9kK]{1}$/.test(rutCompleto))
                return false;
            let tmp = rutCompleto.split('-');
            let digv = tmp[1];
            let rut = tmp[0];
            if (digv == 'K') digv = 'k';

            return (Fn.dv(rut) == digv);
        },
        dv: function (T) {
            let M = 0, S = 1;
            for (; T; T = Math.floor(T / 10))
                S = (S + T % 10 * (9 - M++ % 6)) % 11;
            return S ? S - 1 : 'k';
        }
    }
    return Fn.validaRut(run)
}